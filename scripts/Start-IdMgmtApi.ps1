#!/usr/bin/pwsh

kubectl create -f ./kubernetes/iam-id-mgmt-api-replicaset.yaml

kubectl create -f ./kubernetes/iam-id-mgmt-api-service-public.yaml

# Only needed if we are accessing this serivce directly from outside the cluster
# - more likely, we'll pass through a gateway/ingress
kubectl create -f ./kubernetes/iam-id-mgmt-api-service-nodeport.yaml

