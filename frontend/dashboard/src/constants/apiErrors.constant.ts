import i18next from 'i18next'

export const API_ERRORS = {
  get SERVER()     { return i18next.t('apiErrors.server') },
  get RATE_LIMIT() { return i18next.t('apiErrors.rateLimit') },
  get NOT_FOUND()  { return i18next.t('apiErrors.notFound') },
  get FORBIDDEN()  { return i18next.t('apiErrors.forbidden') },
  get BAD_REQUEST(){ return i18next.t('apiErrors.badRequest') },
  get NETWORK()    { return i18next.t('apiErrors.network') },
  get UNAUTHORIZED(){ return i18next.t('apiErrors.unauthorized') },
}

const BACKEND_KEYS: Record<string, string> = {
  MISSING_PARAMETERS:           'apiErrors.missingParameters',
  SERVER_ERROR:                 'apiErrors.serverError',
  INVALID_USERNAME_OR_PASSWORD: 'apiErrors.invalidUsernameOrPassword',
  NO_ASSIGNED_ROLE:             'apiErrors.noAssignedRole',
  EMAIL_VERIFICATION_FAILED:    'apiErrors.emailVerificationFailed',
  NOT_MATCHING_CONFIRM_PASSWORD:'apiErrors.notMatchingConfirmPassword',
  SET_NEW_PASSWORD_FAILED:      'apiErrors.setNewPasswordFailed',
  REQUEST_PASSWORD_RESET_FAILED:'apiErrors.requestPasswordResetFailed',
  RESET_PASSWORD_FAILED:        'apiErrors.resetPasswordFailed',
  MISSING_TOKEN:                'apiErrors.missingToken',
  INVALID_TOKEN:                'apiErrors.invalidToken',
}

export const BACKEND_ERROR_CODES: Record<string, string> = new Proxy(BACKEND_KEYS, {
  get(target, key: string) {
    return key in target ? i18next.t(target[key]) : undefined
  },
})
