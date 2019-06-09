#!/usr/bin/pwsh

kubectl delete -f ./kubernetes/identity-db-service-public.yaml

kubectl delete -f ./kubernetes/identity-db-statefulset.yaml

kubectl delete -f ./kubernetes/identity-db-service-headless.yaml

kubectl delete -f ./kubernetes/identity-db-persistent-volume-pgsql.yaml








