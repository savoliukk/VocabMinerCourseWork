# Deploy VocabMiner to Azure VPS with Docker Compose

This runbook deploys the coursework demo to an Ubuntu VM with Docker Compose,
PostgreSQL, and Caddy automatic HTTPS.

Target URL:

```text
https://vocabminer.savoliukk.pp.ua
```

## 1. Create the Azure student environment

1. Create or sign in to Azure for Students.
2. Use the `West Europe` region. If the selected VM size is unavailable, use
   `North Europe` as the fallback region.
3. Create one Ubuntu 24.04 LTS virtual machine.
4. Pick a free-tier eligible B1s/B-series size if the portal shows it as
   available for the student subscription.
5. Open only these inbound ports in the Network Security Group:
   - `22/tcp` for SSH
   - `80/tcp` for HTTP/Let's Encrypt validation
   - `443/tcp` for HTTPS
6. Add a low budget alert in Cost Management before leaving the VM running.

Do not create a managed PostgreSQL server for this demo. PostgreSQL runs inside
the Compose stack to keep the deployment small and cheap.

## 2. Point DNS to the VM

Create or update this DNS record after the VM has a public IP:

```text
Type: A
Name: vocabminer
Value: <Azure VM public IP>
```

Wait until the record resolves:

```bash
dig +short vocabminer.savoliukk.pp.ua
```

Caddy can issue the HTTPS certificate only after the DNS record points to the
VM and Azure allows inbound ports `80` and `443`.

## 3. Install Docker Engine on Ubuntu

SSH into the VM, then install Docker from the official Docker apt repository:

```bash
sudo apt update
sudo apt install -y ca-certificates curl
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc

sudo tee /etc/apt/sources.list.d/docker.sources >/dev/null <<EOF
Types: deb
URIs: https://download.docker.com/linux/ubuntu
Suites: $(. /etc/os-release && echo "${UBUNTU_CODENAME:-$VERSION_CODENAME}")
Components: stable
Architectures: $(dpkg --print-architecture)
Signed-By: /etc/apt/keyrings/docker.asc
EOF

sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
sudo systemctl enable --now docker
sudo docker run --rm hello-world
```

Use `sudo docker ...` commands unless you intentionally configure the Docker
user group.

## 4. Deploy the application

Clone the repository and create the environment file:

```bash
git clone https://github.com/savoliukk/VocabMinerCourseWork.git
cd VocabMinerCourseWork
cp VocabMinerCourseWork/.env.example VocabMinerCourseWork/.env
```

Review `VocabMinerCourseWork/.env`. For the coursework demo, these defaults are valid:

```text
DOMAIN=vocabminer.savoliukk.pp.ua
ASPNETCORE_ENVIRONMENT=Development
ApplyMigrations=true
```

If you change `POSTGRES_PASSWORD`, also update the password inside
`ConnectionStrings__DefaultConnection`.

Build and start the stack:

```bash
sudo docker compose -f VocabMinerCourseWork/docker-compose.yml up -d --build
sudo docker compose -f VocabMinerCourseWork/docker-compose.yml ps
```

Check logs if HTTPS or migrations need time:

```bash
sudo docker compose -f VocabMinerCourseWork/docker-compose.yml logs -f api postgres caddy
```

## 5. Verify the demo

From your local machine:

```bash
curl -I https://vocabminer.savoliukk.pp.ua/
```

Open:

```text
https://vocabminer.savoliukk.pp.ua/swagger
```

Seed demo user:

```text
email: student@example.com
password: Password123!
id: 11111111-1111-1111-1111-111111111111
```

Run one representative scenario from `VocabMinerCourseWork.Api.http` through
Swagger before the defense.

## 6. Update or stop the deployment

Update from GitHub:

```bash
git pull
sudo docker compose -f VocabMinerCourseWork/docker-compose.yml up -d --build
sudo docker compose -f VocabMinerCourseWork/docker-compose.yml logs -f api caddy
```

Stop the app without deleting database data:

```bash
sudo docker compose -f VocabMinerCourseWork/docker-compose.yml down
```

Delete containers and the local PostgreSQL volume:

```bash
sudo docker compose -f VocabMinerCourseWork/docker-compose.yml down -v
```

After the coursework defense, stop or delete the Azure VM and associated disk/IP
if you no longer need the public demo.
