# Azure Microservices [Work In Progress]

This project demonstrates microservices in .NET Core with Azure DevOps pipelines.

[TODO] architecture high level diagram

## Build

Prerequisites

* Install .NET 6 SDK and tool kit https://dotnet.microsoft.com/en-us/download/dotnet/6.0

* Install EF core tools
```
dotnet tool update --global dotnet-ef
```

* Install VS Community for Mac Preview Edition (17.0+) which supports .NET Core 6.0

### Generate SQL migrations in EF core

dotnet ef migrations add InitialCreate -s ../Orders.API

## Azure Kubernetes

### Setup Azure Registry

Instructions to setup the registry
```
AZURE_REG_LOCATION="australiaeast"
AZURE_REG_RESOURCE_GROUP_NAME="sfts-boa-dev-aks"
AZURE_REG_NAME="excdemo"
 
# Create a resource group for container registry
az group create --name $ACR_RESOURCE_GROUP_NAME --location $AZURE_REG_LOCATION
 
# Create a container registry that will be where you deploy your image to
az acr create --resource-group $AZURE_REG_RESOURCE_GROUP_NAME --name $AZURE_REG_NAME --sku Basic
```

### Setup Azure DevOps

To setup the Azure DevOps project
```
AZURE_DEVOPS_ORG="SteadfastNANO"
AZURE_DEVOPS_PROJECT_NAME="Azure-Microservices"

az devops project create --organization https://dev.azure.com/$AZURE_DEVOPS_ORG --name $AZURE_DEVOPS_PROJECT_NAME

az devops service-endpoint create 
  --service-endpoint-type docker \
  --organization https://dev.azure.com/$AZURE_DEVOPS_ORG \
  --name myAcrConnection
  --docker-registry-type azure \
  --docker-acr-registry $AZURE_REG_NAME.azurecr.io
  --azure-rm-subscription-id blah-blah \
  --azure-rm-service-principal-id blah-blah \
  --azure-rm-tenant-id blah-blah
```

Generate pipelines for each individual microservice

```
# Install the azure devops extension
az extension add --upgrade -n azure-devops

az pipelines create --name "products.ci" \
    --description "Pipeline for Products microservice" \
    --organization https://dev.azure.com/$AZURE_DEVOPS_ORG \
    --project $AZURE_DEVOPS_PROJECT_NAME \
    --yml-path devops/build/products/ci-pipeline-products-template.yml

# Create a new pipeline (should be repeated each of the microservices)
az pipelines create --name "products.ci" \
    --description "Pipeline for Products microservice" \
    --organization https://dev.azure.com/$AZURE_DEVOPS_ORG \
    --project $AZURE_DEVOPS_PROJECT_NAME \
    --repository https://github.com/pseudonator/Azure-Microservices \
    --branch main \
    --yml-path devops/build/products/ci-pipeline-products-template.yml

az pipelines create --name "portal.ci" \
    --description "Pipeline for Portal microservice" \
    --organization https://dev.azure.com/$AZURE_DEVOPS_ORG \
    --project $AZURE_DEVOPS_PROJECT_NAME \
    --repository Azure-Microservices \
    --repository-type tfsgit \
    --branch master \
    --yml-path devops/build/portal/ci-pipeline-portal-template.yml

az pipelines create --name "orders.ci" \
    --description "Pipeline for Orders microservice" \
    --repository Azure-Microservices \
    --repository-type tfsgit \
    --branch master \
    --yml-path devops/build/orders/ci-pipeline-orders-template.yml

az pipelines create --name "cart.ci" \
    --description "Pipeline for Cart microservice" \
    --repository Azure-Microservices \
    --repository-type tfsgit \
    --branch master \
    --yml-path devops/build/cart/ci-pipeline-cart-template.yml

az pipelines create --name "payments.ci" \
    --description "Pipeline for Payments microservice" \
    --repository Azure-Microservices \
    --repository-type tfsgit \
    --branch master \
    --yml-path devops/build/payments/ci-pipeline-payments-template.yml

az pipelines create --name "notifications.ci" \
    --description "Pipeline for Notifications microservice" \
    --repository Azure-Microservices \
    --repository-type tfsgit \
    --branch master \
    --yml-path devops/build/notifications/ci-pipeline-notifications-template.yml
```