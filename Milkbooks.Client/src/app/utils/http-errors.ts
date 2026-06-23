import { HttpErrorResponse } from '@angular/common/http';

/**
 * Edit this map to customise the message shown for any HTTP status code.
 */
const STATUS_MESSAGES: Record<number, string> = {
  0:   'Could not reach the server — check your connection.',
  400: 'Bad request.',
  401: 'Unauthorised.',
  403: 'Access denied.',
  404: 'No results found.',
  429: 'Rate limit hit — please try again later.',
  500: 'Server error — please try again.',
  502: 'Could not reach the Unsplash API — please try again.',
  503: 'Service unavailable.',
};

export function friendlyHttpError(err: unknown): string {
  if (err instanceof HttpErrorResponse) {
    return STATUS_MESSAGES[err.status] ?? `Unexpected error (${err.status}).`;
  }
  if (err instanceof Error) return err.message;
  return 'An unexpected error occurred.';
}
