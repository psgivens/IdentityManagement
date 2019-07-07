#!/usr/bin/pwsh

# Necessary if we are debugging an app running on our local machine. 
kubectl create -f ./kubernetes/iam-id-mgmt-db-service-nodeport.yaml
