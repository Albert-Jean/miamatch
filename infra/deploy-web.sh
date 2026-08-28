#!/usr/bin/env bash
# Deploie le frontend Angular sur S3 + CloudFront.
#
#   ./infra/deploy-web.sh
#
# Necessite une session AWS active (aws login) et npm.
set -euo pipefail

BUCKET=miamatch-web-987119353333
DISTRIBUTION=E33JYQMGL6YN25
REGION=eu-west-3

cd "$(dirname "$0")/.."

echo "==> Build de production"
npm --prefix web ci --silent
npx --prefix web ng build --configuration production

DIST=web/dist/web/browser
test -f "$DIST/index.html" || { echo "Build introuvable dans $DIST"; exit 1; }

# Les noms de fichiers sont hashes par Angular, donc leur contenu ne change jamais :
# on peut les mettre en cache indefiniment. index.html, lui, pointe vers les hashes
# du jour et doit etre relu a chaque visite, sinon le navigateur sert un index
# perime qui reference des fichiers supprimes.
echo "==> Envoi des fichiers hashes (cache long)"
aws s3 sync "$DIST" "s3://$BUCKET" --region "$REGION" --delete \
  --cache-control "public,max-age=31536000,immutable" \
  --exclude "index.html"

echo "==> Envoi de index.html (sans cache)"
aws s3 cp "$DIST/index.html" "s3://$BUCKET/index.html" --region "$REGION" \
  --cache-control "no-cache,no-store,must-revalidate" \
  --content-type "text/html"

# Le sync a pu supprimer les anciens fichiers, mais CloudFront peut encore en
# servir une copie : on purge index.html pour que la nouvelle version parte tout de suite.
echo "==> Invalidation CloudFront"
ID=$(aws cloudfront create-invalidation --distribution-id "$DISTRIBUTION" \
  --paths "/index.html" "/" --query "Invalidation.Id" --output text)
aws cloudfront wait invalidation-completed --distribution-id "$DISTRIBUTION" --id "$ID"

echo "==> En ligne : https://d1uu986wsttg61.cloudfront.net"
