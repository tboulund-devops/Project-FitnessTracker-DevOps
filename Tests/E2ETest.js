import {selectors} from 'testcafe';

fixture `E2E Test`
    .page `http://localhost:3000`;

test('should display the correct title', async t => {
    const title = await selectors('h1').innerText;
    await t.expect(title).eql('Welcome to My App');
});

test('should navigate to the about page', async t => {
    await t.click(selectors('a').withText('About'));
    const url = await t.eval(() => window.location.href);
    await t.expect(url).contains('/about');
}