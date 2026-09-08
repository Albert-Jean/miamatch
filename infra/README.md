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
  *Run workflow*), enchaine tests -> verification des variables -> images backend -> frontend.

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

Un repo par image, s'ils n'existent pas deja
(`aws ecr describe-repositories --query "repositories[].repositoryName"`) :

```bash
for repo in users-api recipes-api matching-api notifications-api \
            notifications-consumer shoppinglist-api shoppinglist-consumer; do
  aws ecr create-repository --repository-name "miamatch-$repo" --region eu-west-3
done
```

Le pipeline ne cree jamais de repo : une faute de frappe dans une variable doit echouer,
pas fabriquer un depot fantome.

### 4. Variables de repo GitHub

*Settings > Secrets and variables > Actions > onglet Variables.* Ce sont des variables,
pas des secrets : elles ne contiennent aucune donnee sensible.

| Variable | Obligatoire | Valeur |
| --- | --- | --- |
| `AWS_DEPLOY_ROLE_ARN` | oui | `arn:aws:iam::987119353333:role/miamatch-github-deploy` |
| `AWS_REGION` | non | Defaut `eu-west-3` |
| `USERS_API_LAMBDA` | oui | Nom de la fonction Lambda |
| `RECIPES_API_LAMBDA` | oui | idem |
| `MATCHING_API_LAMBDA` | oui | idem |
| `NOTIFICATIONS_API_LAMBDA` | oui | idem |
| `NOTIFICATIONS_CONSUMER_LAMBDA` | oui | idem |
| `SHOPPINGLIST_API_LAMBDA` | oui | idem |
| `SHOPPINGLIST_CONSUMER_LAMBDA` | oui | idem |
| `<SERVICE>_ECR_REPOSITORY` | non | Repo ECR du service ; a defaut, le nom de la Lambda est reutilise |
| `WEB_BUCKET` | non | Defaut `miamatch-web-987119353333` |
| `WEB_CLOUDFRONT_DISTRIBUTION_ID` | non | Defaut `E33JYQMGL6YN25` |

Les noms exacts deja en place se retrouvent avec :

```bash
aws lambda list-functions --region eu-west-3 --query "Functions[].FunctionName" --output table
aws ecr describe-repositories --region eu-west-3 --query "repositories[].repositoryName" --output table
```

Si une variable obligatoire manque, le job `config` echoue en les listant, avant toute
poussee d'image.

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
