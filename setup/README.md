# Setup

These are the steps to set Knative and its dependencies.

## Configuration

Edit [config](config) file for your setup.

## Create a GKE cluster

```sh
./create-gke-cluster
```

## Install Istio & Knative Serving

```sh
./install-serving
```

## Install Knative Eventing

```sh
./install-eventing
```

You probably need a Broker in the default namespace with Knative Eventing.
You can follow instructions in [Broker Creation](../docs/brokercreation.md) page to do that.

## Install Knative GCP

If you intend to read Google Cloud events, install [Knative GCP](https://github.com/google/knative-gcp) components.

There are 2 ways of setting up authentication in Knative GCP:

1. Kubernetes secrets
2. Workload identity (recommended)

Pick one of the mechanisms and use appropriate scripts.

Install Knative GCP:

```sh
# Kubernetes secrets
./install-knative-gcp

# Workload identity
./install-knative-gcp workload
```

Configure a Pub/Sub enabled Service Account for Data Plane:

```sh
# Kubernetes secrets
./install-dataplane-serviceaccount

# Workload identity
./install-dataplane-serviceaccount workload
```

## Install Tekton Pipelines

Install Tekton Pipelines, if you want to run build samples:

```sh
./install-tekton
```

-------
