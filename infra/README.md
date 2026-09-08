# Deploiement

## Ce qui tourne ou

| Composant | Cible AWS |
| --- | --- |
| `*.Api` (Users, Recipes, Matching, Notifications, ShoppingList) | Une Lambda par service, image conteneur, derriere API Gateway (HTTP API) |
| `*.Consumer` (Notifications, ShoppingList) | Une Lambda par service, image conteneur, declenchee par SQS |
| `web/` (Angular) | Bucket S3 prive derriere CloudFront |
| Bases | RDS PostgreSQL |
| Secrets | AWS Secrets Manager, lu au demarrage via `MIAMMATCH_SECRET_ID` |

## Le pipeline

- `.github/workflows/ci.yml` : `dotnet restore/build/test`. Tourne sur chaque pull request,
  et est appele par le workflow de deploiement.
- `.github/workflows/deploy.yml` : sur chaque push vers `master` (ou a la main via
  *Run workflow*), enchaine tests -> verification du role -> images backend -> frontend.

Un push sur `master` produit donc, sans intervention :

1. les tests ;
2. sept images Docker construites depuis la racine du depot, poussees dans ECR taggees
   avec le SHA du commit **et** `latest` ;
3. `aws lambda update-function-code` sur les sept fonctions, puis attente que chacune ait
   fini de basculer ;
4. le build Angular de production, la synchronisation S3 et l'invalidation CloudFront,
   via le meme `deploy-web.sh` que pour un deploiement manuel.

Les deploiements sont serialises (`concurrency: deploy-master`) et le front part apres le
backend, pour ne jamais exposer une interface qui appelle des endpoints absents.

**Rollback** : relancer le workflow sur le commit precedent, ou pointer la fonction sur
l'ancien tag directement :

```bash
aws lambda update-function-code --function-name <fonction> \
  --image-uri <compte>.dkr.ecr.eu-west-3.amazonaws.com/<repo>:<sha-precedent>
```

## Mise en place (une seule fois)

### 1. Fournisseur OIDC GitHub

Rien a faire s'il existe deja (`aws iam list-open-id-connect-providers`) :

```bash
aws iam create-open-id-connect-provider \
  --url https://token.actions.githubusercontent.com \
  --client-id-list sts.amazonaws.com
```

### 2. Role IAM assume par Actions

Aucune cle d'acces n'est stockee dans le depot : Actions presente un jeton OIDC et
recupere des identifiants temporaires. Trust policy (`trust.json`) :

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Federated": "arn:aws:iam::987119353333:oidc-provider/token.actions.githubusercontent.com"
      },
      "Action": "sts:AssumeRoleWithWebIdentity",
      "Condition": {
        "StringEquals": {
          "token.actions.githubusercontent.com:aud": "sts.amazonaws.com",
          "token.actions.githubusercontent.com:sub": "repo:Albert-Jean/miamatch:ref:refs/heads/master"
        }
      }
    }
  ]
}
```

La condition `sub` limite le role a `master` : une pull request depuis un fork ne peut pas
l'assumer, meme si elle modifie le workflow.

```bash
aws iam create-role --role-name miamatch-github-deploy \
  --assume-role-policy-document file://trust.json
```

Permissions (`deploy-policy.json`), au plus juste de ce que fait le pipeline :

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "EcrLogin",
      "Effect": "Allow",
      "Action": "ecr:GetAuthorizationToken",
      "Resource": "*"
    },
    {
      "Sid": "EcrPush",
      "Effect": "Allow",
      "Action": [
        "ecr:BatchCheckLayerAvailability",
        "ecr:BatchGetImage",
        "ecr:CompleteLayerUpload",
        "ecr:GetDownloadUrlForLayer",
        "ecr:InitiateLayerUpload",
        "ecr:PutImage",
        "ecr:UploadLayerPart"
      ],
      "Resource": "arn:aws:ecr:eu-west-3:987119353333:repository/miamatch-*"
    },
    {
      "Sid": "LambdaDeploy",
      "Effect": "Allow",
      "Action": [
        "lambda:GetFunction",
        "lambda:GetFunctionConfiguration",
        "lambda:UpdateFunctionCode"
      ],
      "Resource": "arn:aws:lambda:eu-west-3:987119353333:function:miamatch-*"
    },
    {
      "Sid": "WebBucket",
      "Effect": "Allow",
      "Action": ["s3:ListBucket", "s3:GetObject", "s3:PutObject", "s3:DeleteObject"],
      "Resource": [
        "arn:aws:s3:::miamatch-web-987119353333",
        "arn:aws:s3:::miamatch-web-987119353333/*"
      ]
    },
    {
      "Sid": "Cloudfront",
      "Effect": "Allow",
      "Action": [
        "cloudfront:CreateInvalidation",
        "cloudfront:GetInvalidation",
        "cloudfront:GetDistribution"
      ],
      "Resource": "arn:aws:cloudfront::987119353333:distribution/E33JYQMGL6YN25"
    }
  ]
}
```

Les motifs `miamatch-*` supposent ce prefixe pour les repos ECR et les fonctions Lambda ;
adapter les ARN si les noms reels different.

```bash
aws iam put-role-policy --role-name miamatch-github-deploy \
  --policy-name miamatch-deploy --policy-document file://deploy-policy.json
```

### 3. Repos ECR

Les sept repos existent deja et suivent la convention `miamatch-<service>` :
`miamatch-users-api`, `miamatch-recipes-api`, `miamatch-matching-api`,
`miamatch-notifications-api`, `miamatch-notifications-consumer`,
`miamatch-shoppinglist-api`, `miamatch-shoppinglist-consumer`. Le workflow les vise par
defaut, il n'y a donc rien a faire ici.

Pour un nouveau service :

```bash
aws ecr create-repository --repository-name miamatch-<service> --region eu-west-3
```

Le pipeline ne cree jamais de repo : une faute de frappe doit echouer, pas fabriquer un
depot fantome.

### 4. Variables de repo GitHub

*Settings > Secrets and variables > Actions > onglet Variables.* Ce sont des variables,
pas des secrets : elles ne contiennent aucune donnee sensible.

Une seule est obligatoire, les autres n'existent que pour surcharger un defaut :

| Variable | Obligatoire | Valeur |
| --- | --- | --- |
| `AWS_DEPLOY_ROLE_ARN` | oui | `arn:aws:iam::987119353333:role/miamatch-github-deploy` |
| `AWS_REGION` | non | Defaut `eu-west-3` |
| `<SERVICE>_LAMBDA` | non | Defaut : le nom en place (tableau ci-dessous) |
| `<SERVICE>_ECR_REPOSITORY` | non | Defaut `miamatch-<service>` |
| `WEB_BUCKET` | non | Defaut `miamatch-web-987119353333` |
| `WEB_CLOUDFRONT_DISTRIBUTION_ID` | non | Defaut `E33JYQMGL6YN25` |

Les cibles cablees dans `deploy.yml`. Les deux consumers portent un suffixe `-api` cote
Lambda mais pas cote ECR : les noms sont donc ecrits en entier plutot que derives d'une
regle.

| Service | Repo ECR | Fonction Lambda |
| --- | --- | --- |
| `users-api` | `miamatch-users-api` | `miamatch-users-api` |
| `recipes-api` | `miamatch-recipes-api` | `miamatch-recipes-api` |
| `matching-api` | `miamatch-matching-api` | `miamatch-matching-api` |
| `notifications-api` | `miamatch-notifications-api` | `miamatch-notifications-api` |
| `notifications-consumer` | `miamatch-notifications-consumer` | `miamatch-notifications-consumer-api` |
| `shoppinglist-api` | `miamatch-shoppinglist-api` | `miamatch-shoppinglist-api` |
| `shoppinglist-consumer` | `miamatch-shoppinglist-consumer` | `miamatch-shoppinglist-consumer-api` |

Apres un renommage cote AWS, la variable correspondante evite d'avoir a toucher au
workflow. Pour verifier ce qui existe reellement :

```bash
aws lambda list-functions --region eu-west-3 --query "Functions[].FunctionName" --output table
aws ecr describe-repositories --region eu-west-3 --query "repositories[].repositoryName" --output table
```

## Ce que le pipeline ne fait pas

- **Migrations EF Core.** RDS n'est pas joignable depuis un runner GitHub, et une migration
  automatique sur un push serait irreversible. Elles restent manuelles, avant le push du
  changement qui en depend :
  ```bash
  dotnet ef database update --project src/Services/Users/Users.Infrastructure \
    --startup-project src/Services/Users/Users.Api
  ```
- **Provisionnement.** Lambdas, API Gateway, RDS, SQS, S3 et CloudFront ont ete crees a la
  main ; le pipeline ne fait que remplacer du code. Un `terraform`/CDK serait la suite
  logique si l'environnement doit etre reproductible.
- **Environnement de recette.** Un seul environnement, la production. Un `staging` se
  greffe en dupliquant le job avec d'autres variables, sous un
  [environment](https://docs.github.com/actions/deployment/targeting-different-environments)
  GitHub.

## Developpement local

```bash
docker compose -f infra/docker-compose.yml up -d   # PostgreSQL
dotnet run --project src/Services/Users/Users.Api
npm --prefix web start
```
