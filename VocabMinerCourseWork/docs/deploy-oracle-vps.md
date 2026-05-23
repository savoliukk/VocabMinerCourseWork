# Deploy VocabMiner to Oracle Cloud Always Free

This runbook deploys the coursework demo to an Oracle Cloud Infrastructure
Always Free VM with Docker Compose, PostgreSQL, and Caddy automatic HTTPS.

Target URL:

```text
https://vocabminer.savoliukk.pp.ua
```

## 1. Create the Oracle Cloud account

1. Sign up for Oracle Cloud Free Tier.
2. Choose the home region carefully. Always Free compute resources are created
   in the home region of the tenancy.
3. After sign-up, open the Oracle Cloud Console and stay in the home region.

Oracle can require a payment card for identity verification. Keep the instance
inside the Always Free limits below.

## 2. Create the VM instance

Open:

```text
Compute > Instances > Create instance
```

Use these values:

```text
Name: vm-vocabminer-demo
Image: Canonical Ubuntu 24.04 or Ubuntu 22.04
Shape: VM.Standard.A1.Flex
OCPUs: 1
Memory: 6 GB
Networking: Create new virtual cloud network
Assign public IPv4 address: Yes
SSH keys: Generate key pair or upload your public key
Boot volume: 50 GB, default performance
```

`VM.Standard.A1.Flex` is ARM-based. The VocabMiner Docker images used in this
project are multi-architecture, so the stack can build and run on ARM64.

If Oracle reports `Out of host capacity`, retry later or create a smaller A1
instance. If A1 is not available, an Always Free AMD Micro instance is usually
too small for a comfortable Docker build, so prefer A1 for this project.

## 3. Open public ports in OCI networking

Open the VCN created for the VM, then:

```text
Virtual Cloud Networks > <vcn-name> > Security Lists > Default Security List
```

Add ingress rules:

```text
Source CIDR: 0.0.0.0/0
IP Protocol: TCP
Destination Port Range: 80
Description: HTTP for Caddy and Let's Encrypt
```

```text
Source CIDR: 0.0.0.0/0
IP Protocol: TCP
Destination Port Range: 443
Description: HTTPS for VocabMiner
```

Keep SSH on `22/tcp`. For a temporary coursework demo, public SSH is acceptable
if the VM uses SSH keys. After the demo, stop or delete the instance.

## 4. Point DNS to the VM

Create or update this DNS record after the VM has a public IP:

```text
Type: A
Name: vocabminer
Value: <Oracle VM public IP>
```

Wait until the record resolves:

```bash
dig +short vocabminer.savoliukk.pp.ua
```

Caddy can issue the HTTPS certificate only after DNS points to the VM and ports
`80` and `443` are reachable.

## 5. Connect to the VM

Use the username that matches the image. For Ubuntu images it is usually
`ubuntu`:

```bash
ssh -i <private-key-file> ubuntu@<Oracle VM public IP>
```

Update the system:

```bash
sudo apt update
sudo apt upgrade -y
```

## 6. Install Docker Engine

Install Docker from the official Docker apt repository:

```bash
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

## 7. Deploy the application

Install Git if it is missing:

```bash
sudo apt install -y git
```

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

Build and start the stack:

```bash
sudo docker compose -f VocabMinerCourseWork/docker-compose.yml up -d --build
sudo docker compose -f VocabMinerCourseWork/docker-compose.yml ps
```

Check logs:

```bash
sudo docker compose -f VocabMinerCourseWork/docker-compose.yml logs -f api postgres caddy
```

## 8. Verify the demo

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

## 9. Update or remove the deployment

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

After the coursework defense, stop or delete the VM if you no longer need the
public demo.
