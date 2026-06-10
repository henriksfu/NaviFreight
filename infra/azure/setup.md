# Azure Deployment Setup

One-time setup to get NaviFreight live on Azure. Takes ~10 minutes.
Everything runs on free/near-free tiers — total cost: $0/month.

---

## Prerequisites

- Azure account (free at https://azure.microsoft.com/free)
- Azure CLI installed: https://learn.microsoft.com/en-us/cli/azure/install-azure-cli
  - macOS: `brew install azure-cli`

---

## Step 1 — Log in and create resources

```bash
az login

# Create a resource group (pick any region close to you)
az group create --name navifreight-rg --location eastus

# Create a Free (F1) Linux App Service Plan
az appservice plan create \
  --name navifreight-plan \
  --resource-group navifreight-rg \
  --sku F1 \
  --is-linux

# Create the Web App — replace <yourname> with something unique (e.g. navifreight-henrik)
az webapp create \
  --name navifreight-<yourname> \
  --resource-group navifreight-rg \
  --plan navifreight-plan \
  --runtime "DOTNETCORE:8.0"
```

---

## Step 2 — Generate a JWT secret

Run this to get a secure random key (must be at least 32 characters):

```bash
openssl rand -base64 48
```

Copy the output — you'll use it in the next step.

---

## Step 3 — Set environment variables on the App Service

Replace `navifreight-<yourname>` with your actual app name and paste your JWT key:

```bash
az webapp config appsettings set \
  --name navifreight-<yourname> \
  --resource-group navifreight-rg \
  --settings \
    Jwt__Key="<your-generated-key>" \
    ASPNETCORE_ENVIRONMENT=Production
```

The rest of the config (InMemory mode, issuer, audience) is already set in
`appsettings.Production.json` and doesn't need to be repeated here.

---

## Step 4 — Get the publish profile for GitHub Actions

```bash
az webapp deployment list-publishing-profiles \
  --name navifreight-<yourname> \
  --resource-group navifreight-rg \
  --xml
```

Copy the entire XML output.

---

## Step 5 — Add secrets and variables to GitHub

Go to your GitHub repo → **Settings → Secrets and variables → Actions**.

Add one **Secret**:
| Name | Value |
|------|-------|
| `AZURE_WEBAPP_PUBLISH_PROFILE` | The XML from Step 4 |

Add one **Variable** (not secret):
| Name | Value |
|------|-------|
| `AZURE_WEBAPP_NAME` | `navifreight-<yourname>` |

---

## Step 6 — Push to trigger deploy

```bash
git push origin master
```

GitHub Actions will run CI (build + test), then deploy. Watch the **Actions** tab.
First deploy takes ~3-4 minutes.

Your app will be live at: `https://navifreight-<yourname>.azurewebsites.net`

---

## Notes

- **Free F1 tier** spins down after 20 minutes of inactivity — first request after idle
  takes ~10-15 seconds to cold start. This is normal for the free tier.
- **InMemory mode** — demo data resets on every cold start. That's expected.
- To upgrade to always-on (no cold starts), switch to B1 tier (~$13/month):
  `az appservice plan update --name navifreight-plan --resource-group navifreight-rg --sku B1`
