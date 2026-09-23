#!/bin/bash
# Скрипт деплоя SKI-S. Запускается на сервере после git pull.
# Собирает бэкенд и фронтенд, атомарно обновляет продакшен, перезапускает сервис.

set -e   # остановиться при любой ошибке

REPO_DIR=/home/bro/projects/ski-s
PUBLISH_DIR=/tmp/skis-publish
TARGET_DIR=/home/bro/projects/ski-s/SkiApi/bin/Release/net8.0
SERVICE=skis

echo "================================================"
echo "  Деплой SKI-S: $(date '+%Y-%m-%d %H:%M:%S')"
echo "================================================"

echo ""
echo "[1/7] Обновляем код из Git..."
cd "$REPO_DIR"
git fetch origin
git reset --hard origin/main
echo "      OK: $(git log -1 --oneline)"

echo ""
echo "[2/7] Собираем бэкенд (dotnet publish)..."
rm -rf "$PUBLISH_DIR"
cd "$REPO_DIR/SkiApi"
dotnet publish -c Release -o "$PUBLISH_DIR" --nologo -v q
echo "      OK: бинарник собран"

echo ""
echo "[3/7] Собираем фронтенд (npm run build)..."
cd "$REPO_DIR/frontend"
if [ ! -d "node_modules" ]; then
  echo "      node_modules нет — устанавливаем..."
  npm install --silent
fi
npm run build --silent
echo "      OK: статика собрана"

echo ""
echo "[4/7] Копируем статику в publish..."
mkdir -p "$PUBLISH_DIR/wwwroot"
cp -r "$REPO_DIR/frontend/dist/"* "$PUBLISH_DIR/wwwroot/"
echo "      OK: wwwroot готов"

echo ""
echo "[5/7] Останавливаем сервис..."
sudo systemctl stop "$SERVICE" || true
echo "      OK: сервис остановлен"

echo ""
echo "[6/7] Синхронизируем файлы..."
rsync -a --delete "$PUBLISH_DIR/" "$TARGET_DIR/"
echo "      OK: файлы обновлены"

echo ""
echo "[7/7] Запускаем сервис..."
sudo systemctl start "$SERVICE"
sleep 2
sudo systemctl is-active "$SERVICE" >/dev/null && echo "      OK: сервис активен" || (echo "      ОШИБКА: сервис не поднялся!"; exit 1)

echo ""
echo "================================================"
echo "  Деплой завершён успешно!"
echo "================================================"
