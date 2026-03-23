#!/usr/bin/env bash
set -euo pipefail

server="db"
user="sa"
password="${SA_PASSWORD}"
db_name="${DB_NAME:-PatientReferralDb}"

sqlcmd_bin="/opt/mssql-tools/bin/sqlcmd"
if [ ! -x "$sqlcmd_bin" ]; then
  sqlcmd_bin="/opt/mssql-tools18/bin/sqlcmd"
fi

sqlcmd_extra=""
if "$sqlcmd_bin" -? 2>&1 | grep -q -- " -C "; then
  sqlcmd_extra="-C"
fi

until "$sqlcmd_bin" ${sqlcmd_extra:+$sqlcmd_extra} -S "$server" -U "$user" -P "$password" -Q "SELECT 1" > /dev/null 2>&1; do
  echo "Waiting for SQL Server to be ready..."
  sleep 2
done

echo "SQL Server is ready. Ensuring database '$db_name' exists..."
"$sqlcmd_bin" ${sqlcmd_extra:+$sqlcmd_extra} -S "$server" -U "$user" -P "$password" -Q "IF DB_ID(N'$db_name') IS NULL CREATE DATABASE [$db_name];"

echo "Applying schema to '$db_name'..."
"$sqlcmd_bin" ${sqlcmd_extra:+$sqlcmd_extra} -S "$server" -U "$user" -P "$password" -d "$db_name" -i "/sql/create_tables.sql"

if [ -f "/sql/seed_data.sql" ]; then
  echo "Applying seed data to '$db_name'..."
  "$sqlcmd_bin" ${sqlcmd_extra:+$sqlcmd_extra} -S "$server" -U "$user" -P "$password" -d "$db_name" -i "/sql/seed_data.sql"
fi

echo "Database initialization completed."
