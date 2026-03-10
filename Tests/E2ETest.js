import { Selector } from 'testcafe';

fixture `E2E Test`
    .page `http://89.150.149.43:8030`;

test('Valid credentials', async t => {
    await t.typeText(Selector('#username'), 'test')
        .typeText(Selector('#password'), 'test')
        .click(Selector('button').withText('Log in'));
    await t.expect(Selector('.login-success').exists).ok();
    
    //const url = await t.eval(() => window.location.href);
    //await t.expect(url).contains('/dashboard');
});

// test('should navigate to the about page', async t => {
//     await t.click(selectors('a').withText('About'));
//     const url = await t.eval(() => window.location.href);
//     await t.expect(url).contains('/about');
// });