import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import '@testing-library/jest-dom/vitest';

import App from '../../Frontend/src/App';
import HomePage from '../../Frontend/src/assets/pages/HomePage';
import LoginPage from '../../Frontend/src/assets/pages/LoginPage';
import NavBar from '../../Frontend/src/assets/Components/NavBar';
import OldWorkouts from '../../Frontend/src/assets/pages/OldWorkouts';
import PageNavigation from '../../Frontend/src/assets/Services/PageNavigation';
import ProfilePage from '../../Frontend/src/assets/pages/ProfilePage';
import RegistrationPage from '../../Frontend/src/assets/pages/RegistrationPage';
import Settings from '../../Frontend/src/assets/pages/Settings';

const mockNavigate = vi.fn();

if (typeof localStorage.clear !== 'function') {
  const storage = new Map<string, string>();
  const localStorageMock: Storage = {
    get length() {
      return storage.size;
    },
    clear() {
      storage.clear();
    },
    getItem(key: string) {
      return storage.has(key) ? storage.get(key)! : null;
    },
    key(index: number) {
      return Array.from(storage.keys())[index] ?? null;
    },
    removeItem(key: string) {
      storage.delete(key);
    },
    setItem(key: string, value: string) {
      storage.set(key, value);
    }
  };

  Object.defineProperty(globalThis, 'localStorage', {
    value: localStorageMock,
    configurable: true
  });
}

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate
  };
});

const jsonResponse = (payload: unknown, status = 200) =>
  new Response(JSON.stringify(payload), {
    status,
    headers: { 'Content-Type': 'application/json' }
  });

const textResponse = (payload: string, status = 200) =>
  new Response(payload, {
    status,
    headers: { 'Content-Type': 'text/plain' }
  });

describe('Frontend coverage tests', () => {
  beforeEach(() => {
    mockNavigate.mockReset();
    vi.stubGlobal('fetch', vi.fn());
  });

  it('renders App routes and navbar visibility rules', async () => {
    const fetchMock = vi.mocked(fetch);
    localStorage.setItem('userID', '1');

    fetchMock
      .mockResolvedValueOnce(jsonResponse({
        name: 'Mette',
        timeOfRegistry: '2024-01-01T00:00:00.000Z',
        totalAmountOfWorkouts: 8,
        totalAmountOfTimeTrained: '6h',
        currentStreakDays: 3,
        currentStreakWeeks: 0,
        bestStreakDays: 10,
        bestStreakWeeks: 1,
        favoriteExercise: 'Squat'
      }))
      .mockResolvedValueOnce(jsonResponse({ username: 'mbert', email: 'a@b.com', timeOfRegistration: '2024-01-01T00:00:00.000Z' }))
      .mockResolvedValueOnce(jsonResponse(true));

    const goTo = (path: string) => {
      window.history.pushState({}, '', path);
      window.dispatchEvent(new PopStateEvent('popstate'));
    };

    goTo('/');
    render(<App />);
    expect(screen.queryByRole('button', { name: 'Logout' })).toBeNull();
    expect(await screen.findByRole('heading', { name: 'Login' })).not.toBeNull();

    goTo('/register');
    expect(await screen.findByRole('heading', { name: 'Registration' })).not.toBeNull();
    expect(screen.queryByRole('button', { name: 'Logout' })).toBeNull();

    goTo('/home');
    expect(await screen.findByRole('button', { name: 'Logout' })).not.toBeNull();

    goTo('/profile');
    expect(await screen.findByRole('heading', { name: 'Profile Page' })).not.toBeNull();
  });

  it('covers NavBar links and logout', async () => {
    localStorage.setItem('userID', '10');
    render(
      <MemoryRouter>
        <NavBar />
      </MemoryRouter>
    );

    expect(screen.getByRole('link', { name: 'Homepage' }).getAttribute('href')).toBe('/home');
    expect(screen.getByRole('link', { name: 'Settings' }).getAttribute('href')).toBe('/settings');

    await userEvent.click(screen.getByRole('button', { name: 'Logout' }));
    expect(localStorage.getItem('userID')).toBeNull();
    expect(mockNavigate).toHaveBeenCalledWith('/');
  });

  it('covers PageNavigation and Settings rendering', () => {
    render(
      <MemoryRouter>
        <PageNavigation />
      </MemoryRouter>
    );
    expect(screen.getByRole('link', { name: 'HomePage' }).getAttribute('href')).toBe('/homepage');

    render(<Settings />);
    expect(screen.getByRole('heading', { name: 'Settings' })).not.toBeNull();
  });

  it('covers LoginPage submit success, failure and fetch exception', async () => {
    const fetchMock = vi.mocked(fetch);
    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    );

    await userEvent.type(screen.getByLabelText('Username'), 'john');
    await userEvent.type(screen.getByLabelText('Password'), 'secret');

    let resolveLogin: (value: Response) => void = () => {};
    fetchMock.mockReturnValueOnce(
      new Promise<Response>((resolve) => {
        resolveLogin = resolve;
      })
    );

    await userEvent.click(screen.getByRole('button', { name: 'Log in' }));
    expect(screen.getByRole('button', { name: 'Logging in…' })).not.toBeNull();
    resolveLogin(textResponse('99'));

    await screen.findByText('99 Redirecting to home…');
    expect(localStorage.getItem('userID')).toBe('99');
    expect(mockNavigate).toHaveBeenCalledWith('/home');

    fetchMock.mockResolvedValueOnce(textResponse('', 401));
    await userEvent.click(screen.getByRole('button', { name: 'Log in' }));
    await screen.findByText('Login failed');

    fetchMock.mockRejectedValueOnce(new Error('network'));
    await userEvent.click(screen.getByRole('button', { name: 'Log in' }));
    await screen.findByText('Could not reach the server. Please try again later.');

    await userEvent.click(screen.getByRole('button', { name: 'Register' }));
    expect(mockNavigate).toHaveBeenCalledWith('/register');
  });

  it('covers RegistrationPage success, failure, exception and return', async () => {
    const fetchMock = vi.mocked(fetch);
    render(
      <MemoryRouter>
        <RegistrationPage />
      </MemoryRouter>
    );

    await userEvent.type(screen.getByLabelText('Add Username'), 'john');
    await userEvent.type(screen.getByLabelText('Add Password'), 'secret');
    await userEvent.type(screen.getByLabelText('Add Name'), 'John Doe');
    await userEvent.type(screen.getByLabelText('Add Email'), 'john@example.com');

    fetchMock.mockResolvedValueOnce(textResponse('Created', 200));
    await userEvent.click(screen.getByRole('button', { name: 'Register' }));
    await screen.findByText('Created');
    expect(mockNavigate).toHaveBeenCalledWith('/');

    fetchMock.mockResolvedValueOnce(textResponse('', 400));
    await userEvent.click(screen.getByRole('button', { name: 'Register' }));
    await screen.findByText('Registration failed');

    fetchMock.mockRejectedValueOnce(new Error('network'));
    await userEvent.click(screen.getByRole('button', { name: 'Register' }));
    await screen.findByText('Could not reach the server. Please try again later.');

    await userEvent.click(screen.getByRole('button', { name: 'Return to Login' }));
    expect(mockNavigate).toHaveBeenCalledWith('/');
  });

  it('covers HomePage no-user, success, fallback error, network error and null payload', async () => {
    const fetchMock = vi.mocked(fetch);

    render(<HomePage />);
    await screen.findByText('Please log in');

    localStorage.setItem('userID', '5');
    fetchMock.mockResolvedValueOnce(
      jsonResponse({
        name: 'Jane',
        timeOfRegistry: '2025-06-01T00:00:00.000Z',
        totalAmountOfWorkouts: 3,
        totalAmountOfTimeTrained: '2h',
        currentStreakDays: 4,
        currentStreakWeeks: 0,
        bestStreakDays: 8,
        bestStreakWeeks: 1,
        favoriteExercise: 'Bench Press'
      })
    );
    const successRender = render(<HomePage />);
    await screen.findByRole('heading', { name: 'Profile Overview' });
    expect(screen.getByText('Jane')).not.toBeNull();
    successRender.unmount();

    fetchMock.mockResolvedValueOnce(textResponse('', 500));
    render(<HomePage />);
    await screen.findByText('Failed to load profile');

    fetchMock.mockRejectedValueOnce(new Error('down'));
    render(<HomePage />);
    await screen.findByText('Could not reach the server. Please try again later.');

    fetchMock.mockResolvedValueOnce(jsonResponse(null));
    const nullRender = render(<HomePage />);
    await waitFor(() => {
      expect(nullRender.container.firstChild).toBeNull();
    });
  });

  it('covers OldWorkouts success transform, empty list, backend error and network error', async () => {
    const fetchMock = vi.mocked(fetch);
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    localStorage.setItem('userID', '7');

    fetchMock.mockResolvedValueOnce(
      jsonResponse([
        {
          workoutID: 1,
          dateOfWorkout: '2024-04-20T00:00:00.000Z',
          sets: [
            { exerciseID: 1, exerciseName: 'Bench Press', reps: 8, weight: 100 },
            { exerciseID: 1, exerciseName: 'Bench Press', reps: 6, weight: 105 },
            { exerciseID: 2, exerciseName: null, reps: 10, weight: 80 }
          ]
        }
      ])
    );

    render(<OldWorkouts />);
    await screen.findByText('Bench Press');
    expect(screen.getByText('Exercise 2')).not.toBeNull();
    expect(screen.getByText('Set 1: 8 reps at 100 kg')).not.toBeNull();

    fetchMock.mockResolvedValueOnce(jsonResponse([]));
    render(<OldWorkouts />);
    await screen.findByText('No workouts found.');

    fetchMock.mockResolvedValueOnce(textResponse('', 500));
    render(<OldWorkouts />);
    await screen.findByText('Failed to load workouts');

    fetchMock.mockRejectedValueOnce(new Error('offline'));
    render(<OldWorkouts />);
    await screen.findByText('Could not reach the server. Please try again later.');
    expect(consoleSpy).toHaveBeenCalled();
  });

  it('covers ProfilePage loading branches, toggle fallback, and update email flows', async () => {
    const fetchMock = vi.mocked(fetch);
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

    const phase1 = render(<ProfilePage />);
    await screen.findByText('Please log in');
    phase1.unmount();

    localStorage.setItem('userID', '12');

    fetchMock
      .mockResolvedValueOnce(textResponse('', 500))
      .mockResolvedValueOnce(jsonResponse(true));
    const phase2 = render(<ProfilePage />);
    await screen.findByText('Failed to load profile');
    phase2.unmount();

    fetchMock.mockRejectedValueOnce(new Error('profile down')).mockResolvedValueOnce(jsonResponse(true));
    const phase3 = render(<ProfilePage />);
    await screen.findByText('Could not reach the server. Please try again later.');
    phase3.unmount();

    fetchMock
      .mockResolvedValueOnce(jsonResponse({ username: 'fit-user', email: 'old@mail.com', timeOfRegistration: '2024-01-01T00:00:00.000Z' }))
      .mockResolvedValueOnce(textResponse('toggle down', 500));
    const phase4 = render(<ProfilePage />);
    await screen.findByText('fit-user');
    expect(screen.queryByRole('button', { name: 'Save Email' })).toBeNull();
    expect(consoleSpy).toHaveBeenCalled();
    phase4.unmount();

    fetchMock
      .mockResolvedValueOnce(jsonResponse({ username: 'fit-user', email: 'same@mail.com', timeOfRegistration: '2024-01-01T00:00:00.000Z' }))
      .mockResolvedValueOnce(jsonResponse(true));
    const phase5 = render(<ProfilePage />);
    await screen.findByDisplayValue('same@mail.com');

    const sameEmailButton = await screen.findByRole('button', { name: 'Save Email' });
    expect((sameEmailButton as HTMLButtonElement).disabled).toBe(true);

    const emailInput = screen.getByDisplayValue('same@mail.com');
    await userEvent.clear(emailInput);
    await userEvent.type(emailInput, 'new@mail.com');

    fetchMock.mockResolvedValueOnce(textResponse('', 500));
    await userEvent.click(screen.getByRole('button', { name: 'Save Email' }));
    await screen.findByText('Failed to update email');
    phase5.unmount();

    fetchMock
      .mockResolvedValueOnce(jsonResponse({ username: 'fit-user', email: 'same@mail.com', timeOfRegistration: '2024-01-01T00:00:00.000Z' }))
      .mockResolvedValueOnce(jsonResponse(true));
    const phase6 = render(<ProfilePage />);
    await screen.findByDisplayValue('same@mail.com');

    const emailInputAgain = screen.getByDisplayValue('same@mail.com');
    await userEvent.clear(emailInputAgain);
    await userEvent.type(emailInputAgain, 'new@mail.com');

    fetchMock.mockRejectedValueOnce(new Error('put down'));
    await userEvent.click(screen.getByRole('button', { name: 'Save Email' }));
    await screen.findByText('Could not reach the server. Please try again later.');
    phase6.unmount();

    fetchMock
      .mockResolvedValueOnce(jsonResponse({ username: 'fit-user', email: 'same@mail.com', timeOfRegistration: '2024-01-01T00:00:00.000Z' }))
      .mockResolvedValueOnce(jsonResponse(true));
    render(<ProfilePage />);
    await screen.findByDisplayValue('same@mail.com');

    const emailInputSuccess = screen.getByDisplayValue('same@mail.com');
    await userEvent.clear(emailInputSuccess);
    await userEvent.type(emailInputSuccess, 'new@mail.com');

    fetchMock
      .mockResolvedValueOnce(textResponse('', 200))
      .mockResolvedValueOnce(jsonResponse({ username: 'fit-user', email: 'new@mail.com', timeOfRegistration: '2024-01-01T00:00:00.000Z' }));
    await userEvent.click(screen.getByRole('button', { name: 'Save Email' }));

    await waitFor(() => {
      expect(fetchMock).toHaveBeenCalledWith('/api/user/APIUser/UpdateEmail?userID=12&newEmail=new%40mail.com', {
        method: 'PUT'
      });
    });
  });

  it('covers explicit loading indicator text for App/Home route', async () => {
    const fetchMock = vi.mocked(fetch);
    localStorage.setItem('userID', '77');

    let resolveFetch: (value: Response) => void = () => {};
    fetchMock.mockReturnValueOnce(
      new Promise<Response>((resolve) => {
        resolveFetch = resolve;
      })
    );

    window.history.pushState({}, '', '/home');
    render(<App />);
    expect(await screen.findByText('Loading profile…')).not.toBeNull();

    resolveFetch(
      jsonResponse({
        name: 'Delayed',
        timeOfRegistry: '2024-01-01T00:00:00.000Z',
        totalAmountOfWorkouts: 1,
        totalAmountOfTimeTrained: '1h',
        currentStreakDays: 1,
        currentStreakWeeks: 0,
        bestStreakDays: 1,
        bestStreakWeeks: 0,
        favoriteExercise: 'Row'
      })
    );

    await screen.findByText('Delayed');
  });

  it('covers loading text toggles on login and registration secondary buttons', async () => {
    const fetchMock = vi.mocked(fetch);
    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    );

    await userEvent.type(screen.getByLabelText('Username'), 'u');
    await userEvent.type(screen.getByLabelText('Password'), 'p');

    let resolveLogin: (value: Response) => void = () => {};
    fetchMock.mockReturnValueOnce(
      new Promise<Response>((resolve) => {
        resolveLogin = resolve;
      })
    );

    fireEvent.submit(screen.getByRole('button', { name: 'Log in' }).closest('form') as HTMLFormElement);
    expect((screen.getByRole('button', { name: 'Navigation to RegistrationPage' }) as HTMLButtonElement).disabled).toBe(true);
    resolveLogin(textResponse('10'));
    await screen.findByText('10 Redirecting to home…');

    render(
      <MemoryRouter>
        <RegistrationPage />
      </MemoryRouter>
    );

    await userEvent.type(screen.getAllByLabelText('Add Username')[0], 'u');
    await userEvent.type(screen.getAllByLabelText('Add Password')[0], 'p');
    await userEvent.type(screen.getByLabelText('Add Name'), 'n');
    await userEvent.type(screen.getByLabelText('Add Email'), 'a@a.com');

    let resolveRegister: (value: Response) => void = () => {};
    fetchMock.mockReturnValueOnce(
      new Promise<Response>((resolve) => {
        resolveRegister = resolve;
      })
    );

    fireEvent.submit(screen.getAllByRole('button', { name: 'Register' })[1].closest('form') as HTMLFormElement);
    expect((screen.getByRole('button', { name: 'Navigation to LoginPage' }) as HTMLButtonElement).disabled).toBe(true);
    resolveRegister(textResponse('ok', 200));
    await screen.findByText('ok');
  });
});

