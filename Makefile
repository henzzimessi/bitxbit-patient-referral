.PHONY: up down reset status logs test test-e2e test-all ci

up:
	powershell -ExecutionPolicy Bypass -File ./scripts/stack.ps1 up

down:
	powershell -ExecutionPolicy Bypass -File ./scripts/stack.ps1 down

reset:
	powershell -ExecutionPolicy Bypass -File ./scripts/stack.ps1 reset

status:
	powershell -ExecutionPolicy Bypass -File ./scripts/stack.ps1 status

logs:
	powershell -ExecutionPolicy Bypass -File ./scripts/stack.ps1 logs

test:
	powershell -ExecutionPolicy Bypass -File ./scripts/stack.ps1 test

test-e2e:
	powershell -ExecutionPolicy Bypass -File ./scripts/stack.ps1 test-e2e

test-all:
	powershell -ExecutionPolicy Bypass -File ./scripts/stack.ps1 test-all

ci:
	powershell -ExecutionPolicy Bypass -File ./scripts/stack.ps1 ci
