#!/bin/bash
# Cria o repositório privado ProjetosIA no GitHub e configura o remote.
# Uso: ./setup-private-repo.sh <SEU_GITHUB_TOKEN>
set -e

TOKEN="${1:-$GITHUB_TOKEN}"
REPO_NAME="ProjetosIA"
GITHUB_USER="rafchase"
DESCRIPTION="Projetos desenvolvidos com Claude"

if [ -z "$TOKEN" ]; then
  echo "Uso: $0 <github_token>"
  echo "  ou defina a variável de ambiente GITHUB_TOKEN"
  echo ""
  echo "Crie um token em: https://github.com/settings/tokens/new"
  echo "  - Permissão necessária: repo (para criar repositório privado)"
  exit 1
fi

echo "==> Criando repositório privado $GITHUB_USER/$REPO_NAME..."
RESPONSE=$(curl -sf -X POST "https://api.github.com/user/repos" \
  -H "Authorization: token $TOKEN" \
  -H "Accept: application/vnd.github+json" \
  -d "{\"name\":\"$REPO_NAME\",\"private\":true,\"description\":\"$DESCRIPTION\"}" 2>&1)

HTML_URL=$(echo "$RESPONSE" | python3 -c "import sys,json; print(json.load(sys.stdin)['html_url'])" 2>/dev/null || true)

if [ -z "$HTML_URL" ]; then
  echo "Aviso: repositório pode já existir, continuando..."
  HTML_URL="https://github.com/$GITHUB_USER/$REPO_NAME"
fi

echo "==> Configurando remote 'origin' para o novo repositório..."
git remote set-url origin "https://github.com/$GITHUB_USER/$REPO_NAME.git"

echo "==> Fazendo push das branches para o GitHub..."
git push -u origin master
git push -u origin --all 2>/dev/null || true

echo ""
echo "Concluído! Repositório disponível em: $HTML_URL"
