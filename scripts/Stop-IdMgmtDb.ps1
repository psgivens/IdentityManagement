#!/usr/bin/pwsh

kubectl delete -f ./kubernetes/iam-id-mgmt-db-statefulset.yaml

kubectl delete pvc/data-iam-id-mgmt-db-0

kubectl delete -f ./kubernetes/iam-id-mgmt-db-persistent-volume-pgsql.yaml

kubectl delete -f ./kubernetes/iam-id-mgmt-api-configmap.yaml
