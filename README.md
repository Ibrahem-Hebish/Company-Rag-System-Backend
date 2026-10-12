# CompanyRagSystem

**An AI-powered company knowledge management system built with ASP.NET Core and Retrieval-Augmented Generation (RAG).**

CompanyRagSystem enables companies to organize their internal knowledge by department and query their documents using natural language. It combines semantic search with locally hosted foundation models to retrieve relevant information and generate context-grounded answers.

The goal is to make internal company knowledge easier to access while keeping document processing and AI inference within a locally hosted environment.

## Features

- **Company and Department Management** — Organize company information into departments.
- **Document Management** — Upload and manage internal knowledge documents.
- **Document Ingestion Pipeline** — Process documents into chunks, enrich them with metadata, generate embeddings, and store them for retrieval.
- **Semantic Search** — Find relevant document chunks based on the meaning of a query rather than exact keyword matches.
- **AI-Powered Question Answering** — Generate answers using retrieved document context.
- **Local AI Inference** — Run chat and embedding models locally through Ollama.
- **Vector Storage** — Store and search document embeddings using PostgreSQL and pgvector.
- **Asynchronous Processing** — Process document ingestion in the background rather than performing the entire pipeline during the upload request.

## Tech Stack

| Technology              | Purpose                              |
| ----------------------- | ------------------------------------ |
| ASP.NET Core Web API 10 | Backend API                          |
| PostgreSQL              | Relational data storage              |
| pgvector                | Vector storage and similarity search |
| Ollama                  | Local model hosting and inference    |
| Llama 3.2 3B            | Chat and answer generation           |
| Qwen3 Embedding 8B      | Text embedding generation            |
| Angular                 | Frontend                             |

## How RAG Works

CompanyRagSystem uses Retrieval-Augmented Generation to answer questions based on a company's internal documents.

### 1. Document Ingestion

When a document is uploaded, it enters an ingestion pipeline:

1. Read and extract the document's content.
2. Split the content into smaller chunks.
3. Generate vector embeddings for the chunks.
4. Store the chunks, embeddings, and associated metadata for retrieval.

### 2. Semantic Search

When a user submits a question:

1. Generate an embedding for the query.
2. Search the vector store for semantically relevant document chunks.
3. Retrieve the most relevant context for the question.

### 3. Answer Generation

The retrieved context is provided to the chat model along with instructions to answer using the available evidence.

The system is designed to avoid unsupported assumptions and indicate when the retrieved information is insufficient to answer a question.

## Architecture Overview

The application separates the user-facing API from background document processing.

```text
Document Upload
      |
      v
ASP.NET Core Web API
      |
      v
Document Storage
      |
      v
Background Ingestion Pipeline
      |
      v
Document Reading
      |
      v
   Chunking
      |
      v
Embedding Generation (Ollama)
      |
      v
PostgreSQL + pgvector


User Question
      |
      v
ASP.NET Core Web API
      |
      v
Query Embedding
      |
      v
Vector Similarity Search
      |
      v
Retrieved Document Chunks
      |
      v
Llama 3.2 3B
      |
      v
Context-Grounded Answer
```

## Getting Started

### Prerequisites

Before running the application, ensure you have the following installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/)
- [pgvector](https://github.com/pgvector/pgvector)
- [Ollama](https://ollama.com/)
- [Node.js](https://nodejs.org/) and npm, if running the Angular frontend

### Download the Models

Start Ollama and download the models used by the application:

```bash
ollama pull llama3.2:3b
ollama pull qwen3-embedding:8b
```

Make sure Ollama is running and the models are available before using the AI features.

### Clone the Repository

```bash
git clone <YOUR_REPOSITORY_URL>
cd CompanyRagSystem
```

### Configure the Application

Configure the database connection string and other required settings in your local application configuration.

Do not commit passwords, API secrets, or other sensitive configuration values to source control.

Ensure that PostgreSQL is configured with the pgvector extension and that the application has the necessary database permissions.

### Run the Backend

From the solution directory, restore dependencies and run the API:

```bash
dotnet restore
dotnet run --project <PATH_TO_API_PROJECT>
```

Replace the project path with the actual path to your API project.

Check the application output for the API URL and available API documentation.

### Run the Frontend

If you want to run the Angular frontend, navigate to its project directory and execute:

```bash
npm install
ng serve
```

The frontend will be available at the local URL reported by Angular CLI.

## Example Use Case

Imagine a company that stores its HR policies in an internal knowledge base.

A user asks:

> What is the company's work-from-home policy?

The system retrieves relevant passages from the HR documents and provides them to the language model to generate an answer grounded in the available information.

This allows employees to query internal documentation without manually searching through entire files.

## Future Improvements

Potential areas for further development include:

- Hybrid retrieval combining vector similarity with keyword search.
- Reranking retrieved chunks to improve relevance.
- Automated evaluation of retrieval quality and generated answers.
- Source citations and document-level evidence in answers.

## Live Demo Link

https://drive.google.com/file/d/1u43c8EeZ_Mu5KKctc7pUGwsOjyBWbIG2/view?usp=drive_link
