import { Selector } from 'testcafe';

fixture `E2E Test`
    .page `http://89.150.149.43:8030`;

test('Valid credentials', async t => {
    await t.typeText(Selector('#username'), 'test')
        .typeText(Selector('#password'), 'test')
        .click(Selector('button').withText('Log in'));
    const url = await t.eval(() => window.location.href);
    await t.expect(url).contains('/home');
});

