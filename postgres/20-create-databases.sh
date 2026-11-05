#!/usr/bin/env bash
set -e

# Вариант 3 — Car Rental System (имя скрипта схемы db-v3.sql)
export VARIANT="v3"
export SCRIPT_PATH=/docker-entrypoint-initdb.d/
export PGPASSWORD=postgres
psql -f "$SCRIPT_PATH/scripts/db-$VARIANT.sql"
