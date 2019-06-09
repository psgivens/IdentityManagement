#!/usr/bin/pwsh

kubectl create -f ./kubernetes/identity-db-persistent-volume-pgsql.yaml

kubectl create -f ./kubernetes/identity-db-service-headless.yaml

kubectl create -f ./kubernetes/identity-db-statefulset.yaml

kubectl create -f ./kubernetes/identity-db-service-public.yaml







