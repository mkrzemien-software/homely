# Infrastruktura AWS dla Aplikacji Angular + .NET 8 z Terraform

## Kontekst projektu
Stwórz kompletną infrastrukturę AWS dla aplikacji webowej składającej się z:
- **Frontend**: Angular (Single Page Application)
- **Backend**: .NET 8 w kontenerze Docker
- **Baza danych**: Supabase Cloud (zewnętrzna)
- **Uwierzytelnianie**: Supabase Cloud
- **Ruch**: mały
- **Budżet**: minimalny
- **CI/CD**: GitHub Actions

## Wymagania architektury

### 1. Frontend (Angular)
- Hosting na **S3 bucket** skonfigurowany jako static website
- **CloudFront** jako CDN z następującymi cechami:
  - Cache dla plików statycznych
  - Kompresja Gzip/Brotli
  - Przekierowanie HTTP → HTTPS
  - Obsługa SPA routing (wszystkie ścieżki → index.html)
- **Certificate Manager** - certyfikat SSL dla domeny
- **Route 53** lub instrukcje do konfiguracji DNS w OVH

### 2. Backend (.NET 8 w Docker)
- **ECR (Elastic Container Registry)** - prywatne repozytorium dla obrazu Docker
- **ECS Fargate** z konfiguracją:
  - 1 task definition dla backendu .NET 8
  - 0.25 vCPU, 512 MB RAM (minimum dla .NET)
  - Service z desired count = 1 (bez autoskalowania)
  - Health checks
  - Logi w CloudWatch
- **Application Load Balancer** (ALB):
  - Publiczny dostęp
  - SSL termination
  - Target group dla ECS tasks
  - Health check endpoint
- **Security Groups** z regułami:
  - ALB: ruch z internetu (80, 443)
  - ECS tasks: ruch tylko z ALB

### 3. Sieć (VPC)
- VPC z CIDR 10.0.0.0/16
- 2 Availability Zones dla wysokiej dostępności
- Public subnets (2x) - dla ALB
- Private subnets (2x) - dla ECS tasks
- Internet Gateway
- NAT Gateway (lub NAT Instance dla oszczędności) - dla ruchu wychodzącego z private subnets
- Route tables

### 4. Bezpieczeństwo
- **IAM Roles**:
  - ECS Task Execution Role (dostęp do ECR, CloudWatch Logs)
  - ECS Task Role (minimalne uprawnienia dla aplikacji + odczyt Parameter Store)
  - GitHub Actions Role (deploy do ECR i ECS + zapis do Parameter Store)
  - Parameter Store access policy z zasadą least privilege (tylko parametry projektu)
- **AWS Systems Manager Parameter Store** dla zmiennych środowiskowych i sekretów:
  - Supabase URL (SecureString)
  - Supabase Anon Key (SecureString)
  - Backend configuration (String)
  - Frontend environment variables
  - Inne configuration secrets
  - Użyj hierarchii: /{project_name}/{environment}/* (np. /homely/prod/supabase_url)

### 5. CI/CD (GitHub Actions)
Wygeneruj przykładowe pliki workflow:
- **Frontend workflow** (.github/workflows/deploy-frontend.yml):
  - Build Angular (npm run build)
  - Upload do S3
  - Invalidate CloudFront cache
- **Backend workflow** (.github/workflows/deploy-backend.yml):
  - Build obrazu Docker
  - Push do ECR
  - Update ECS service
  - Health check po deploymencie

### 6. Monitoring i logi
- **CloudWatch Log Groups** dla:
  - ECS tasks
  - ALB access logs
- **CloudWatch Alarms** (opcjonalne, dla minimalnego kosztu):
  - ALB 5xx errors
  - ECS task failures

## Struktura plików Terraform

Stwórz modularną strukturę:

```
terraform/
├── main.tf                 # Główna konfiguracja
├── variables.tf            # Zmienne wejściowe
├── outputs.tf              # Outputy (endpoints, URLs)
├── terraform.tfvars        # Wartości zmiennych (nie commitować do repo!)
├── backend.tf              # S3 backend dla state
├── providers.tf            # AWS provider
│
├── modules/
│   ├── networking/         # VPC, subnets, NAT, etc.
│   ├── frontend/           # S3, CloudFront, ACM
│   ├── backend/            # ECS, ALB, ECR
│   ├── security/           # Security Groups, IAM
│   ├── parameters/         # Parameter Store configuration
│   └── monitoring/         # CloudWatch, Alarms
│
└── environments/
    ├── dev/
    └── prod/
```

## Zmienne do zdefiniowania

Uwzględnij następujące zmienne w `variables.tf`:
- `project_name` (string) - nazwa projektu
- `environment` (string) - dev/staging/prod
- `aws_region` (string) - domyślnie eu-central-1 lub eu-west-1
- `domain_name` (string) - domena z OVH
- `frontend_domain` (string) - subdomena dla frontendu (np. app.domena.pl)
- `backend_domain` (string) - subdomena dla backendu (np. api.domena.pl)
- `docker_image_tag` (string) - tag obrazu Docker
- `supabase_url` (string, sensitive)
- `supabase_anon_key` (string, sensitive)
- `github_repo` (string) - nazwa repozytorium GitHub

## Optymalizacje kosztów

Zastosuj następujące optymalizacje dla minimalnego budżetu:
1. **ECS Fargate Spot** - do 70% taniej (z automatycznym fallback na On-Demand)
2. **S3 Intelligent-Tiering** dla frontendu
3. **CloudFront Free Tier** - pierwsze 1TB transferu gratis
4. **Single NAT Gateway** zamiast 2 (jeśli HA nie jest krytyczne)
5. **t4g.nano NAT Instance** zamiast NAT Gateway (jeszcze tańsze)
6. **CloudWatch Logs retention** - 7 dni zamiast indefinite
7. **ALB access logs** - wyłączone lub krótka retencja
8. **Parameter Store Standard tier** - darmowy (do 10,000 parametrów)

## Outputs

Po wykonaniu `terraform apply`, wyświetl:
- CloudFront distribution URL
- ALB DNS name
- ECR repository URI
- Parameter Store paths (ścieżki do parametrów)
- Instrukcje konfiguracji DNS w OVH
- Przykładowe komendy GitHub Actions secrets
- Komendy AWS CLI do ustawienia parametrów (np. `aws ssm put-parameter --name "/homely/prod/supabase_url" --value "..." --type SecureString`)

## Dokumentacja

Dodaj README.md z:
1. Prerequisities (AWS CLI, Terraform, Docker)
2. Kroki wdrożenia
3. Konfiguracja GitHub Actions secrets
4. Konfiguracja DNS w OVH
5. Komendy do zarządzania infrastrukturą
6. Troubleshooting

## Bezpieczeństwo plików

Dodaj do `.gitignore` :
```
*.tfstate
*.tfstate.backup
.terraform/
terraform.tfvars
*.pem
*.key
.env
```

## Dodatkowe wymagania

1. **Wszystkie zasoby tagowane** z:
   - Project
   - Environment
   - ManagedBy: "Terraform"
   - CostCenter

2. **Remote state** w S3 + DynamoDB lock

3. **Parameter Store configuration**:
   - Hierarchia parametrów: `/{project_name}/{environment}/{parameter_name}`
   - SecureString dla sekretów (Supabase credentials)
   - String dla non-sensitive config
   - ECS task definition pobiera zmienne z Parameter Store
   - Przykłady parametrów:
     - `/homely/prod/supabase_url` (SecureString)
     - `/homely/prod/supabase_anon_key` (SecureString)
     - `/homely/prod/database_connection_string` (SecureString)
     - `/homely/prod/backend_log_level` (String)
     - `/homely/prod/cors_origins` (String)

4. **Backup plan** - instrukcje jak zachować state i odtworzyć infrastrukturę

5. **Rollback strategy** - jak szybko wrócić do poprzedniej wersji

---

## Format odpowiedzi

Wygeneruj:
1. Wszystkie pliki Terraform z pełnym kodem (katalog "infrastructure")
2. Pliki GitHub Actions workflows
3. README.md z instrukcjami
4. Przykładowy Dockerfile dla backendu .NET 8
5. Skrypt inicjalizacyjny (init.sh) do pierwszego uruchomienia

Kod powinien być gotowy do użycia, z komentarzami po angielsku, zgodny z best practices Terraform i AWS Well-Architected Framework.