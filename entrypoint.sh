#!/bin/bash
set -e

echo ">> Aguardando banco de dados..."
until nc -z -v -w30 db 3306
do
  echo "Aguardando conexão com MySQL..."
  sleep 2
done

echo ">> Iniciando aplicação..."
dotnet Rota.API.dll