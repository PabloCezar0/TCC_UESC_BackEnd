#!/bin/bash
set -e

echo ">> Aguardando banco..."
until nc -z db 3306; do
  echo "Esperando MySQL..."
  sleep 2
done

echo ">> Rodando migrations..."
dotnet ef database update -p Rota.Infra.Data -s Rota.API
echo ">> Migrations concluídas."
