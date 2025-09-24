<h1>Jaké funkce tu jsou?</h1>
    <h2>Shrnutí projektu</h2>
    <p>Projekt zasílám přesně ve 14. den, protože jsem si chtěl dát záležet na kvalitě implementace.</p>
    <p>
        Během práce jsem využíval ChatGPT pro inspiraci a nápovědu, což mi umožnilo rychleji řešit komplikované problémy. Uvědomuji si však, že bych měl více času věnovat vlastnímu programování, abych si koncepty plně osvojil.
    </p>
    <h3>Splněné funkce podle zadání</h3>
    <ul>
        <li>Registrace nového uživatele (jméno, příjmení, email, heslo).</li>
        <li>První registrovaný uživatel se automaticky stává <strong>superadminem</strong>.</li>
        <li>Přihlášení a odhlášení uživatele.</li>
        <li>Dvoufaktorová autentizace přes <strong>TOTP</strong>.</li>
        <li>Menu obsahuje více stránek s <strong>Lorem Ipsum</strong> textem pro všechny uživatele.</li>
        <li>
            Administrátor má k dispozici administraci uživatelů:
            <ul>
                <li>výpis uživatelů</li>
                <li>editace uživatele (jméno, role)</li>
                <li>možnost odebrat administrátorská práva</li>
                <li>volitelně odstranění uživatele</li>
            </ul>
        </li>
    </ul>
    <h3>Přidané funkce nad rámec zadání</h3>
    <ul>
        <li>Vstupy ukládané do databáze jsou <strong>ošetřeny proti XSS</strong> s důrazem na čistotu kódu.</li>
        <li><strong>Logovací systém</strong> dostupný pro superadmina.</li>
        <li>Produkty v e-shopu se zobrazují <strong>dynamicky pomocí JavaScriptu</strong> (ochota přejít na Node.js v budoucnu).</li>
        <li><ul>
                <li>Pro dynamické načítání produktů jsem použil JavaScript. I když s touto technologií nemám hluboké zkušenosti, díky studiu syntaxe a podpoře ChatGPT se mi podařilo vytvořit funkční řešení. Projekt mi umožnil lépe pochopit principy dynamického zobrazování obsahu.</li>
            </ul>
        </li>
        <li>Dokončený <strong>košík objednávek</strong> ukládaný do cookies včetně ochrany proti manipulaci s cenami.</li>
        <li><strong>Živé (dynamické) generování náhodných objednávek</strong> pro superadmina a admina.</li>
        <li><strong>Administrace produktů, včetně cen</strong> jen pro superadmina  – formulář (modal) se načítá dynamicky při kliknutí.</li>
        
    <h3>Funkce, které zatím chybí</h3>
    <ul>
        <li>Stav objednávky a možnost nastavení filtrů u produktů</li>
        <li>Původní nápad na interní sociální síť s „vlastním obsahem“ uživatelů nebyl realizován. Myšlenka superadmina, který spravuje adminy jako moderátory obsahu, mě napadla až pozdě, a <strong>požadavek zadání, aby uživatel viděl vlastní obsah, mi navíc připadal zavádějící</strong>. Nevznikl by totiž unikátní obsah pro každého uživatele, protože by vyžadoval pokročilé algoritmy; v praxi by šlo o jednoduchý RSS feed s dynamickým řazením příspěvků od nejnovějších po nejstarší, <strong>o „vlastní obsah“ pro různé uživatele by se tedy nejednalo</strong>.</li>
        <li>Doslova na poslední chvíli mě ale napadlo, že by se každému uživateli mohly ukládat seznamy odběrů do sloučeného RSS – technicky vzato by tak vznikl částečně „vlastní obsah“ pro každého uživatele.</li>
    </ul>
    ## Jak projekt spustit

Pro spuštění projektu doporučuji použít pokročilé editory jako Visual Studio Community nebo JetBrains Rider.
Alternativně lze použít i Visual Studio Code s doinstalovaným rozšířením C# Dev Kit, který nainstaluje .NET SDK včetně nástroje dotnet.

1. Naklonujte repozitář  
   `git clone https://github.com/danixek/PojistakNET.git`  
   `cd PojistakNET`
2. Ověřte připojení k databázi v souboru `appsettings.json`  
   (pokud používáte výchozí nastavení, přeskočte)
3. Sestavte projekt:  
   `dotnet build`  
   Spuštěním se zkontroluje struktura projektu a automaticky se stáhnou závislosti - NuGet balíčky.
4. Proveďte migraci databáze:
   ```bash příkazy  
   dotnet ef database update
5. Spusťte projekt:  
   `dotnet run`
   
> 💡 **Poznámka:** Pokud se příkaz `dotnet ef` nezdaří, je pravděpodobně potřeba doinstalovat EF CLI nástroj:  
`dotnet tool install --global dotnet-ef`

Po úspěšném spuštění se v konzoli objeví adresa (např. https://localhost:7204).
Otevřete ji v prohlížeči – projekt by měl být dostupný.
Ve Visual Studiu Community nebo Rideru se aplikace často spustí automaticky s otevřením prohlížeče.
