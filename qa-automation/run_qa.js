const puppeteer = require('puppeteer');
const fs = require('fs');
const path = require('path');

const targetApi = process.argv[2];

if (!targetApi) {
    console.log("Uso:\nnode run_qa.js products\nnode run_qa.js users\nnode run_qa.js cart");
    process.exit(1);
}

if (targetApi !== 'products' && targetApi !== 'users' && targetApi !== 'cart') {
    console.log("Por ahora solo está soportado: products, users, cart");
    console.log("Uso:\nnode run_qa.js products\nnode run_qa.js users\nnode run_qa.js cart");
    process.exit(1);
}

const config = {
    products: {
        name: 'Products.API',
        swaggerUrl: 'https://localhost:61008/swagger/index.html',
        docsPath: '../docs/products'
    },
    users: {
        name: 'Users.API',
        swaggerUrl: 'https://localhost:61011/swagger/index.html',
        docsPath: '../docs/users'
    },
    cart: {
        name: 'Cart.API',
        swaggerUrl: 'https://localhost:61016/swagger/index.html',
        docsPath: '../docs/cart'
    }
};

const apiConfig = config[targetApi];
const SWAGGER_URL = apiConfig.swaggerUrl;

const delay = ms => new Promise(res => setTimeout(res, ms));

// Clear a textarea using React native setter (works for body params)
async function clearTextArea(page, selector) {
    await page.evaluate((sel) => {
        const el = document.querySelector(sel);
        const nativeSetter = Object.getOwnPropertyDescriptor(window.HTMLTextAreaElement.prototype, "value").set;
        nativeSetter.call(el, '');
        el.dispatchEvent(new Event('input', { bubbles: true }));
    }, selector);
}

// Clear and type into an input field using keyboard events (works for path params)
async function clearAndTypeInput(page, selector, value) {
    const input = await page.$(selector);
    await input.click({ clickCount: 3 }); // select all
    await delay(100);
    await page.keyboard.press('Backspace');
    await delay(100);
    await input.type(value, { delay: 0 });
    await delay(200);
    // Also dispatch change event to make sure Swagger picks it up
    await page.evaluate((sel) => {
        const el = document.querySelector(sel);
        el.dispatchEvent(new Event('change', { bubbles: true }));
    }, selector);
}

async function takeElementScreenshot(page, selector, name) {
    const fullPath = path.resolve(__dirname, apiConfig.docsPath, name);
    console.log(`Taking element screenshot: ${fullPath}`);
    const element = await page.$(selector);
    if (element) {
        await element.screenshot({ path: fullPath });
    } else {
        console.error(`Element not found for screenshot: ${selector}`);
    }
}

// Opens an endpoint panel fresh: click header, click Try it out only if not already active
async function openEndpoint(page, sel) {
    const header = await page.$(`${sel} .opblock-summary`);
    await header.click();
    await delay(800);
    const tryOutBtn = await page.$(`${sel} .try-out__btn`);
    if (tryOutBtn) {
        const btnText = await page.evaluate(el => el.textContent.trim(), tryOutBtn);
        if (btnText !== 'Cancel') {
            await tryOutBtn.click();
            await delay(800);
        }
    }
    return header;
}

// Closes an endpoint panel
async function closeEndpoint(page, sel) {
    const header = await page.$(`${sel} .opblock-summary`);
    await header.click();
    await delay(400);
}

async function runProducts(page) {
    const uniqueId = Date.now();
    const productName = `Producto QA Screenshot ${uniqueId}`;

    const validProduct = {
        "nombre": productName,
        "descripcion": "Producto creado para capturas de evidencia",
        "precio": 100,
        "stock": 10,
        "categoria": "Electrónica"
    };

    const selGet    = '#operations-Products-get_api_Products';
    const selPost   = '#operations-Products-post_api_Products';
    const selGetId  = '#operations-Products-get_api_Products__id_';
    const selPut    = '#operations-Products-put_api_Products__id_';
    const selDel    = '#operations-Products-delete_api_Products__id_';

    // ── 1. GET /api/Products ──
    console.log('1. GET /api/Products');
    await openEndpoint(page, selGet);
    let executeBtn = await page.$(`${selGet} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selGet, 'screenshots/get-all-200.png');
    await closeEndpoint(page, selGet);

    // ── 2. POST /api/Products válido ──
    console.log('2. POST /api/Products válido');
    await openEndpoint(page, selPost);
    await clearTextArea(page, `${selPost} .body-param__text`);
    await page.type(`${selPost} .body-param__text`, JSON.stringify(validProduct, null, 2));
    executeBtn = await page.$(`${selPost} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selPost, 'screenshots/post-201.png');

    // Extract ID
    const responseText = await page.$eval(`${selPost} .responses-table .highlight-code`, el => el.innerText);
    const jsonMatch = responseText.match(/\{[\s\S]*\}/);
    let createdId = '';
    if (jsonMatch) {
        try {
            const parsed = JSON.parse(jsonMatch[0]);
            createdId = parsed.id;
            console.log(`Created Product ID: ${createdId}`);
        } catch (e) {
            console.error('Could not parse JSON', jsonMatch[0]);
        }
    }
    await closeEndpoint(page, selPost);
    if (!createdId) throw new Error('Failed to create product, cannot continue');

    // ── 3. GET /api/Products/{id} válido ──
    console.log('3. GET /api/Products/{id} válido');
    await openEndpoint(page, selGetId);
    await clearAndTypeInput(page, `${selGetId} input[placeholder="id"]`, createdId);
    executeBtn = await page.$(`${selGetId} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selGetId, 'screenshots/get-by-id-200.png');
    await closeEndpoint(page, selGetId);

    // ── 4. PUT /api/Products/{id} ──
    console.log('4. PUT /api/Products/{id}');
    await openEndpoint(page, selPut);
    await clearAndTypeInput(page, `${selPut} input[placeholder="id"]`, createdId);
    const updateProduct = {
        "nombre": `Producto QA Screenshot Actualizado ${uniqueId}`,
        "descripcion": "Producto actualizado para capturas",
        "precio": 150,
        "stock": 8,
        "categoria": "Electrónica"
    };
    await clearTextArea(page, `${selPut} .body-param__text`);
    await page.type(`${selPut} .body-param__text`, JSON.stringify(updateProduct, null, 2));
    executeBtn = await page.$(`${selPut} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selPut, 'screenshots/put-200.png');
    await closeEndpoint(page, selPut);

    // ── 5. POST duplicado (antes del DELETE para que el producto exista) ──
    console.log('5. POST duplicado (PRD-003)');
    await openEndpoint(page, selPost);
    await clearTextArea(page, `${selPost} .body-param__text`);
    const duplicateProduct = {
        "nombre": `Producto QA Screenshot Actualizado ${uniqueId}`,
        "descripcion": "Intento de duplicado",
        "precio": 200,
        "stock": 5,
        "categoria": "Electrónica"
    };
    await page.type(`${selPost} .body-param__text`, JSON.stringify(duplicateProduct, null, 2));
    executeBtn = await page.$(`${selPost} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selPost, 'errors/prd-003-duplicate-product.png');
    await closeEndpoint(page, selPost);

    // ── 6. POST inválido (PRD-002) ──
    console.log('6. POST inválido (PRD-002)');
    await openEndpoint(page, selPost);
    const invalidProduct = {
        "nombre": "",
        "descripcion": "Producto inválido",
        "precio": 0,
        "stock": -1,
        "categoria": ""
    };
    await clearTextArea(page, `${selPost} .body-param__text`);
    await page.type(`${selPost} .body-param__text`, JSON.stringify(invalidProduct, null, 2));
    executeBtn = await page.$(`${selPost} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selPost, 'errors/prd-002-invalid-data.png');
    await closeEndpoint(page, selPost);

    // ── 7. DELETE /api/Products/{id} ──
    console.log('7. DELETE /api/Products/{id}');
    await openEndpoint(page, selDel);
    await clearAndTypeInput(page, `${selDel} input[placeholder="id"]`, createdId);
    executeBtn = await page.$(`${selDel} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selDel, 'screenshots/delete-204.png');
    await closeEndpoint(page, selDel);

    // ── 8. GET producto inexistente (zeroed GUID → 404, PRD-001) ──
    console.log('8. GET producto inexistente (PRD-001)');
    await openEndpoint(page, selGetId);
    await clearAndTypeInput(page, `${selGetId} input[placeholder="id"]`, '00000000-0000-0000-0000-000000000000');
    executeBtn = await page.$(`${selGetId} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selGetId, 'errors/prd-001-product-not-found.png');
    await closeEndpoint(page, selGetId);
}

async function runUsers(page) {
    const uniqueId = Date.now();
    const selRegister = '#operations-Users-post_api_Users_register';
    const selLogin = '#operations-Users-post_api_Users_login';

    const validEmail = `qa-user-${uniqueId}@test.com`;
    const validPassword = 'Password123!';

    const validUser = {
        "nombre": "Usuario",
        "apellido": "QA",
        "email": validEmail,
        "password": validPassword
    };

    // ── A. POST /api/Users/register válido ──
    console.log('A. POST /api/Users/register válido');
    await openEndpoint(page, selRegister);
    await clearTextArea(page, `${selRegister} .body-param__text`);
    await page.type(`${selRegister} .body-param__text`, JSON.stringify(validUser, null, 2));
    let executeBtn = await page.$(`${selRegister} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selRegister, 'screenshots/register-201.png');
    await closeEndpoint(page, selRegister);

    // ── B. POST /api/Users/login válido ──
    console.log('B. POST /api/Users/login válido');
    const validLogin = {
        "email": validEmail,
        "password": validPassword
    };
    await openEndpoint(page, selLogin);
    await clearTextArea(page, `${selLogin} .body-param__text`);
    await page.type(`${selLogin} .body-param__text`, JSON.stringify(validLogin, null, 2));
    executeBtn = await page.$(`${selLogin} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selLogin, 'screenshots/login-200.png');
    await closeEndpoint(page, selLogin);

    // ── C. Register duplicado ──
    console.log('C. Register duplicado (USR-001)');
    await openEndpoint(page, selRegister);
    await clearTextArea(page, `${selRegister} .body-param__text`);
    await page.type(`${selRegister} .body-param__text`, JSON.stringify(validUser, null, 2));
    executeBtn = await page.$(`${selRegister} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selRegister, 'errors/usr-001-email-duplicado.png');
    await closeEndpoint(page, selRegister);

    // ── D. Register inválido ──
    console.log('D. Register inválido (USR-002)');
    const invalidUser = {
        "nombre": "",
        "apellido": "",
        "email": "email-invalido",
        "password": ""
    };
    await openEndpoint(page, selRegister);
    await clearTextArea(page, `${selRegister} .body-param__text`);
    await page.type(`${selRegister} .body-param__text`, JSON.stringify(invalidUser, null, 2));
    executeBtn = await page.$(`${selRegister} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selRegister, 'errors/usr-002-datos-invalidos.png');
    await closeEndpoint(page, selRegister);

    // ── E. Login credenciales incorrectas ──
    console.log('E. Login credenciales incorrectas (USR-003)');
    const invalidLogin = {
        "email": validEmail,
        "password": "PasswordIncorrecta123!"
    };
    await openEndpoint(page, selLogin);
    await clearTextArea(page, `${selLogin} .body-param__text`);
    await page.type(`${selLogin} .body-param__text`, JSON.stringify(invalidLogin, null, 2));
    executeBtn = await page.$(`${selLogin} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selLogin, 'errors/usr-003-credenciales-incorrectas.png');
    await closeEndpoint(page, selLogin);

    // ── F. Bloqueo por 3 intentos fallidos ──
    console.log('F. Bloqueo por 3 intentos fallidos (USR-004)');
    const blockedEmail = `qa-blocked-${uniqueId}@test.com`;
    const userToBlock = {
        "nombre": "Usuario",
        "apellido": "Bloqueado",
        "email": blockedEmail,
        "password": validPassword
    };

    // Crear el usuario
    await openEndpoint(page, selRegister);
    await clearTextArea(page, `${selRegister} .body-param__text`);
    await page.type(`${selRegister} .body-param__text`, JSON.stringify(userToBlock, null, 2));
    executeBtn = await page.$(`${selRegister} .execute`);
    await executeBtn.click();
    await delay(2000);
    await closeEndpoint(page, selRegister);

    // Intentar login con clave incorrecta 3 veces y captura en la 3ra (o 4ta dependiendo de la lógica de negocio, por las dudas hacemos 4)
    const blockLogin = {
        "email": blockedEmail,
        "password": "WrongPassword1!"
    };
    
    await openEndpoint(page, selLogin);
    for (let i = 1; i <= 4; i++) {
        await clearTextArea(page, `${selLogin} .body-param__text`);
        await page.type(`${selLogin} .body-param__text`, JSON.stringify(blockLogin, null, 2));
        executeBtn = await page.$(`${selLogin} .execute`);
        await executeBtn.click();
        await delay(2000);
        if (i === 4 || i === 3) {
            // El requerimiento dice: "Al tercer intento o luego del tercer intento: 403 Forbidden errorCode = USR-004"
            // Capturar en cada uno por si acaso, sobrescribe
            await takeElementScreenshot(page, selLogin, 'errors/usr-004-usuario-bloqueado.png');
        }
    }
    await closeEndpoint(page, selLogin);
}

async function runCart(page) {
    const uniqueId = Date.now();
    const productName = `Producto QA Cart ${uniqueId}`;
    const validProduct = {
        "nombre": productName,
        "descripcion": "Producto creado para pruebas de carrito",
        "precio": 100,
        "stock": 10,
        "categoria": "QA"
    };

    console.log('Creating valid product via HTTP in Products.API...');
    process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";
    let productId = '';
    try {
        const response = await fetch('https://localhost:61008/api/products', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(validProduct)
        });
        const product = await response.json();
        productId = product.id;
        console.log(`Created Product ID: ${productId}`);
    } catch (e) {
        console.error('Failed to create product via HTTP', e);
        throw new Error('Cannot continue without product');
    }

    const userId = '11111111-1111-1111-1111-111111111111';
    const missingUserId = '22222222-2222-2222-2222-222222222222';
    const missingProductId = '00000000-0000-0000-0000-000000000000';

    const selPostItem = '#operations-Cart-post_api_Cart__userId__items';
    const selGetCart = '#operations-Cart-get_api_Cart__userId_';
    const selPutItem = '#operations-Cart-put_api_Cart__userId__items__productId_';
    const selDelItem = '#operations-Cart-delete_api_Cart__userId__items__productId_';
    const selDelCart = '#operations-Cart-delete_api_Cart__userId_';

    // ── A. POST /api/Cart/{userId}/items válido ──
    console.log('A. POST /api/Cart/{userId}/items válido');
    await openEndpoint(page, selPostItem);
    await clearAndTypeInput(page, `${selPostItem} input[placeholder="userId"]`, userId);
    await clearTextArea(page, `${selPostItem} .body-param__text`);
    const addItemValid = { "productId": productId, "cantidad": 1 };
    await page.type(`${selPostItem} .body-param__text`, JSON.stringify(addItemValid, null, 2));
    let executeBtn = await page.$(`${selPostItem} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selPostItem, 'screenshots/add-item-200.png');
    await closeEndpoint(page, selPostItem);

    // ── B. GET /api/Cart/{userId} ──
    console.log('B. GET /api/Cart/{userId}');
    await openEndpoint(page, selGetCart);
    await clearAndTypeInput(page, `${selGetCart} input[placeholder="userId"]`, userId);
    executeBtn = await page.$(`${selGetCart} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selGetCart, 'screenshots/get-cart-200.png');
    await closeEndpoint(page, selGetCart);

    // ── C. PUT /api/Cart/{userId}/items/{productId} ──
    console.log('C. PUT /api/Cart/{userId}/items/{productId}');
    await openEndpoint(page, selPutItem);
    await clearAndTypeInput(page, `${selPutItem} input[placeholder="userId"]`, userId);
    await clearAndTypeInput(page, `${selPutItem} input[placeholder="productId"]`, productId);
    await clearTextArea(page, `${selPutItem} .body-param__text`);
    const updateItemValid = { "cantidad": 2 };
    await page.type(`${selPutItem} .body-param__text`, JSON.stringify(updateItemValid, null, 2));
    executeBtn = await page.$(`${selPutItem} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selPutItem, 'screenshots/update-item-200.png');
    await closeEndpoint(page, selPutItem);

    // ── D. DELETE /api/Cart/{userId}/items/{productId} ──
    console.log('D. DELETE /api/Cart/{userId}/items/{productId}');
    await openEndpoint(page, selDelItem);
    await clearAndTypeInput(page, `${selDelItem} input[placeholder="userId"]`, userId);
    await clearAndTypeInput(page, `${selDelItem} input[placeholder="productId"]`, productId);
    executeBtn = await page.$(`${selDelItem} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selDelItem, 'screenshots/delete-item-204.png');
    await closeEndpoint(page, selDelItem);

    // ── E. DELETE /api/Cart/{userId} ──
    console.log('E. DELETE /api/Cart/{userId}');
    // Add an item again first so the cart exists
    console.log('   -> Re-adding item to clear cart');
    await openEndpoint(page, selPostItem);
    await clearAndTypeInput(page, `${selPostItem} input[placeholder="userId"]`, userId);
    await clearTextArea(page, `${selPostItem} .body-param__text`);
    await page.type(`${selPostItem} .body-param__text`, JSON.stringify(addItemValid, null, 2));
    executeBtn = await page.$(`${selPostItem} .execute`);
    await executeBtn.click();
    await delay(2000);
    await closeEndpoint(page, selPostItem);

    await openEndpoint(page, selDelCart);
    await clearAndTypeInput(page, `${selDelCart} input[placeholder="userId"]`, userId);
    executeBtn = await page.$(`${selDelCart} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selDelCart, 'screenshots/clear-cart-204.png');
    await closeEndpoint(page, selDelCart);

    // ── F. GET carrito inexistente (CRT-001) ──
    console.log('F. GET carrito inexistente (CRT-001)');
    await openEndpoint(page, selGetCart);
    await clearAndTypeInput(page, `${selGetCart} input[placeholder="userId"]`, missingUserId);
    executeBtn = await page.$(`${selGetCart} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selGetCart, 'errors/crt-001-cart-not-found.png');
    await closeEndpoint(page, selGetCart);

    // ── G. POST producto inexistente (CRT-002) ──
    console.log('G. POST producto inexistente (CRT-002)');
    await openEndpoint(page, selPostItem);
    await clearAndTypeInput(page, `${selPostItem} input[placeholder="userId"]`, userId);
    await clearTextArea(page, `${selPostItem} .body-param__text`);
    const addItemMissingProduct = { "productId": missingProductId, "cantidad": 1 };
    await page.type(`${selPostItem} .body-param__text`, JSON.stringify(addItemMissingProduct, null, 2));
    executeBtn = await page.$(`${selPostItem} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selPostItem, 'errors/crt-002-product-not-found.png');
    await closeEndpoint(page, selPostItem);

    // ── H. POST stock insuficiente (CRT-003) ──
    console.log('H. POST stock insuficiente (CRT-003)');
    await openEndpoint(page, selPostItem);
    await clearAndTypeInput(page, `${selPostItem} input[placeholder="userId"]`, userId);
    await clearTextArea(page, `${selPostItem} .body-param__text`);
    const addItemHighQuantity = { "productId": productId, "cantidad": 9999 };
    await page.type(`${selPostItem} .body-param__text`, JSON.stringify(addItemHighQuantity, null, 2));
    executeBtn = await page.$(`${selPostItem} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selPostItem, 'errors/crt-003-stock-insufficient.png');
    await closeEndpoint(page, selPostItem);

    // ── I. POST cantidad inválida (CRT-004) ──
    console.log('I. POST cantidad inválida (CRT-004)');
    await openEndpoint(page, selPostItem);
    await clearAndTypeInput(page, `${selPostItem} input[placeholder="userId"]`, userId);
    await clearTextArea(page, `${selPostItem} .body-param__text`);
    const addItemInvalidQuantity = { "productId": productId, "cantidad": 0 };
    await page.type(`${selPostItem} .body-param__text`, JSON.stringify(addItemInvalidQuantity, null, 2));
    executeBtn = await page.$(`${selPostItem} .execute`);
    await executeBtn.click();
    await delay(2000);
    await takeElementScreenshot(page, selPostItem, 'errors/crt-004-invalid-quantity.png');
    await closeEndpoint(page, selPostItem);
}

async function run() {
    console.log('Launching browser...');
    const browser = await puppeteer.launch({
        ignoreHTTPSErrors: true,
        args: ['--no-sandbox', '--disable-setuid-sandbox']
    });

    const page = await browser.newPage();
    await page.setViewport({ width: 1280, height: 2000 });

    const docsDir = path.resolve(__dirname, apiConfig.docsPath);
    if (!fs.existsSync(docsDir)) fs.mkdirSync(docsDir, { recursive: true });
    if (!fs.existsSync(path.join(docsDir, 'screenshots'))) fs.mkdirSync(path.join(docsDir, 'screenshots'), { recursive: true });
    if (!fs.existsSync(path.join(docsDir, 'errors'))) fs.mkdirSync(path.join(docsDir, 'errors'), { recursive: true });

    try {
        console.log('Loading Swagger...');
        await page.goto(SWAGGER_URL, { waitUntil: 'networkidle2' });
        await delay(2000);

        if (targetApi === 'products') {
            await runProducts(page);
        } else if (targetApi === 'users') {
            await runUsers(page);
        } else if (targetApi === 'cart') {
            await runCart(page);
        }
    } catch (err) {
        console.error('Error during execution:', err);
    } finally {
        await browser.close();
        console.log('Done!');
    }
}

run();
