import type { Exercise } from "./Exercise";
export interface Workout {
    id: number;
    date: string;
    exercises: Exercise[];
}
