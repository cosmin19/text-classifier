# Simple text classifier
Big Data Project

## Tema
Proiectul are ca scop realizarea unei aplicatii web care sa permita asignarea automata a unei arii tematice unui articol stiintific uploadat in aplicatie. Aceasta incadrare intr-o arie tematica va fi facuta pe baza analizei titlului, abstractului si al continutului respectivului articol.

## Introducere
Proiectul realizat consta intr-o aplicatie web de tip Client-Server. Utilizatorul, cu ajutorul interfetei grafice, incarca un document de tip TXT/PDF/DOC/DOCX pe server. Serverul, la primirea unui fisier, verifica daca documentul este valid, extrage tot textul din fisier, face o sumarizarea a acestuia pe baza unor cuvinte cheie apoi, pe baza acestui rezumat, se face clasificarea intr-o arie tematica.
Totodata, utilizatorul are posibilitatea de a vedea fisierele uploadate anterior, poate vedea iar ariile tematice ale acestor articole si de asemenea le poate descarca iarasi, acestea fiind stocate in baza de date.

## Tehnologii folosite
**Back-end:** C# (.NET Framework)

**Front-end:** Razor (HTML + CSS)

**Baza de date:** MSSQL / ORM: EntityFramework

**Librarii:**
  1. iTextSharp: Folosita pentru manipularea fisierelor PDF
  2. Microsoft.Office.Interop.Word: Pentru manipularea fisierelor Word
  3. OpenTextSummarizer: Pentru sumarizarea textului citit din documentele uploadate
  
**Machine Learning Service:** uClassify

## Open Text Summarizer
OTS este o librarie de .NET open-source, oferita de CodePlex1 – un proiect open-source sustinut si oferit de Microsoft. OTS permite sumarizarea paginilor web sau a altor documente (PDF, Word etc.) scotand in evidenta cele mai importante concepte si idei din document. A fost dezvoltat intial pentru platforma Linux. Avand un mare succes si fiind un tool foarte folosit, acesta a inceput sa fie adaugat in majoritatea distributiilor Linux.
Folosind OTS, se pot determina rapid conceptele principale dintr-un document. Sumarizarea poate fi realizata dupa anumiti parametri setati de utilizator, precum procentul sau numarul de propozitii in care sa se faca.

## uClassify
uClassify este un serviciu Machine Learning gratuit folosit in crearea, manipularea sau clasificarea textelor. Poate fi folosit in clasificarea textelor dupa sentimente, limba, topicuri, tinalitate etc.
Clasificarea textelor dupa topicuri foloseste un model deja antrenat, oferit de Open Directory Project2 (cunoscut si ca DMOZ). Acesta a fost un catalog de Internet disponibil in mai multe limbi. Acesta a ajuns cel mai bine cotat catalog, datorita atat numarului mare de categorii si subcategorii, cat si a numarului urias de resurse continute. Acesta are in prezent peste 4.5 milioane de site-uri indexate si aproximativ 600.000 de categorii.
