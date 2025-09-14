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
    </ul>
    <h3>Funkce, které zatím chybí</h3>
    <ul>
        <li>Dokončený <strong>košík objednávek</strong> ukládaný do cookies včetně ochrany proti manipulaci s cenami.</li>
        <li><strong>Živé generování náhodných objednávek</strong> pro superadmina (s možností sledovat dynamicky načítané objednávky).</li>
        <li>Původní nápad na interní sociální síť se "vlastním obsahem" uživatelů nebyl realizován.</li>
    </ul>
