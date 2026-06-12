const puppeteer = require('puppeteer');
(async () => {
    const browser = await puppeteer.launch({ ignoreHTTPSErrors: true, args: ['--no-sandbox'] });
    const page = await browser.newPage();
    await page.goto('https://localhost:61008/swagger/index.html', {waitUntil: 'networkidle2'});
    
    let header = await page.$('#operations-Products-post_api_Products .opblock-summary');
    await header.click();
    await new Promise(r => setTimeout(r, 1000));
    
    let tryOutBtn = await page.$('#operations-Products-post_api_Products .try-out__btn');
    if (tryOutBtn) await tryOutBtn.click();
    await new Promise(r => setTimeout(r, 1000));

    await page.evaluate(() => {
        const el = document.querySelector('#operations-Products-post_api_Products .body-param__text');
        el.value = '';
        el.dispatchEvent(new Event('input', {bubbles:true}));
    });
    
    const validProduct = {
          "nombre": "Producto QA Antigravity",
          "descripcion": "Producto creado desde Swagger",
          "precio": 100,
          "stock": 10,
          "categoria": "Electrónica"
    };
    await page.type('#operations-Products-post_api_Products .body-param__text', JSON.stringify(validProduct, null, 2));

    let executeBtn = await page.$('#operations-Products-post_api_Products .execute');
    await executeBtn.click();
    
    await new Promise(r => setTimeout(r, 2000));
    
    const responseText = await page.$eval('#operations-Products-post_api_Products .responses-table .highlight-code', el => el.innerText);
    console.log("RESPONSE TEXT IS:");
    console.log(responseText);
    
    await browser.close();
})();
