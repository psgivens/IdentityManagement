#!/usr/bin/pwsh

kubectl delete -f ./kubernetes/iam-id-mgmt-api-service-nodeport.yaml

kubectl delete -f ./kubernetes/iam-id-mgmt-api-service-public.yaml

kubectl delete -f ./kubernetes/iam-id-mgmt-api-replicaset.yaml





