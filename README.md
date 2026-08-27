# Proyecto Final Bootcamp - API .NET & Kubernetes

Este repositorio contiene la solución unificada que integra una API desarrollada en .NET 10 persistida en PostgreSQL, empaquetada con Docker, automatizada por un pipeline de CI/CD (GitHub Actions), y orquestada mediante manifiestos de Kubernetes y plantillas de Helm.

---

## 🚀 Guía de Despliegue de Punta a Punta

### 1. Prerrequisitos locales
Asegúrate de tener instalado y activo tu clúster local:
```bash
minikube start
```

### 2. Despliegue de la Infraestructura Base (PostgreSQL, Seq y API)
Aplica los manifiestos base almacenados en la carpeta `k8s/` ejecutando el siguiente comando:
```bash
kubectl apply -f k8s/
```

### 3. Instalación Dinámica mediante Helm
Para renderizar e instalar la solución utilizando el Chart de Helm según el entorno deseado:

* **Entorno Desarrollo (Dev):**
  ```bash
  helm install mi-api ./chart -f ./chart/values-dev.yaml
  ```
* **Entorno Aseguramiento de Calidad (QA):**
  ```bash
  helm install mi-api ./chart -f ./chart/values-qa.yaml
  ```

---

## 🛠️ Evidencias de Operación en Kubernetes

### Autorecuperación (Self-Healing)
El ReplicaSet del Deployment garantiza la disponibilidad constante de 2 réplicas de la API. 
* **Prueba realizada:** Se eliminó un Pod de forma manual usando el comando:
  ```bash
  kubectl delete pod <nombre-del-pod-api>
  ```
* **Resultado:** Kubernetes detectó la caída de forma inmediata y el ReplicaSet procedió a crear un Pod idéntico de reemplazo en cuestión de segundos para mantener el estado deseado.

### Escalado Declarativo
Para escalar horizontalmente la infraestructura de la API de forma declarativa, se ejecutó:
```bash
kubectl scale deployment mi-api-chart-api --replicas=4
```
Esto permite gestionar picos de tráfico incrementando dinámicamente el número de réplicas activas bajo demanda.

---

## 📊 Logging Centralizado y Correlación (Seq)
La API utiliza **Serilog** para generar logs estructurados en JSON inyectando un identificador de correlación único (`requestId`). 
* **Acceso local a Seq:** Expón el panel de visualización ejecutando:
  ```bash
  minikube service seq-service --url
  ```
* **Búsqueda de Eventos:** Al consultar la propiedad estructurada `requestId` dentro del buscador de Seq, es posible filtrar y seguir el flujo secuencial de una misma transacción HTTP, incluso si sus subprocesos fueron atendidos de forma distribuida entre múltiples réplicas físicas de la API.

---

## 🔄 Automatización CI/CD (GitHub Actions)
El archivo `.github/workflows/pipeline.yml` está configurado con validación estricta en el orden óptimo de ejecución:
1. `checkout` ➡️ 2. `restore` ➡️ 3. `build` ➡️ 4. `test`

* **Validación de Fallos:** Se encuentra documentado en el historial de la pestaña **Actions** un check de compilación en rojo provocado de forma intencional mediante un error de código sintáctico, seguido de su respectiva corrección confirmada con un check exitoso en color verde.
