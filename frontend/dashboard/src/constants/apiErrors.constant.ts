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
  MISSING_PARAMETERS:                 'apiErrors.missingParameters',
  SERVER_ERROR:                       'apiErrors.serverError',
  INVALID_USERNAME_OR_PASSWORD:       'apiErrors.invalidUsernameOrPassword', // NOSONAR: i18n key, not a credential
  NO_ASSIGNED_ROLE:                   'apiErrors.noAssignedRole',
  EMAIL_VERIFICATION_FAILED:          'apiErrors.emailVerificationFailed',
  NOT_MATCHING_CONFIRM_PASSWORD:      'apiErrors.notMatchingConfirmPassword', // NOSONAR: i18n key, not a credential
  SET_NEW_PASSWORD_FAILED:            'apiErrors.setNewPasswordFailed',       // NOSONAR: i18n key, not a credential
  REQUEST_PASSWORD_RESET_FAILED:      'apiErrors.requestPasswordResetFailed', // NOSONAR: i18n key, not a credential
  RESET_PASSWORD_FAILED:              'apiErrors.resetPasswordFailed',        // NOSONAR: i18n key, not a credential
  MISSING_TOKEN:                      'apiErrors.missingToken',
  INVALID_TOKEN:                      'apiErrors.invalidToken',
  CREATE_PASSWORD_FAILED:             'apiErrors.createPasswordFailed',       // NOSONAR: i18n key, not a credential
  FAMILY_NOT_FOUND:                   'apiErrors.familyNotFound',
  USER_NOT_FOUND:                     'apiErrors.userNotFound',
  FAMILY_NOT_MEMBER:                  'apiErrors.familyNotMember',
  FAMILY_ALREADY_MEMBER:              'apiErrors.familyAlreadyMember',
  FAMILY_CANNOT_INVITE_DEFAULT:       'apiErrors.familyCannotInviteDefault',
  FAMILY_CANNOT_REMOVE_DEFAULT:       'apiErrors.familyCannotRemoveDefault',
  FAMILY_CANNOT_REMOVE_SELF_HEAD:     'apiErrors.familyCannotRemoveSelfHead',
  FAMILY_CANNOT_CHANGE_OWN_ROLE:      'apiErrors.familyCannotChangeOwnRole',
  FAMILY_CANNOT_ARCHIVE_DEFAULT:      'apiErrors.familyCannotArchiveDefault',
  FAMILY_CANNOT_LEAVE_DEFAULT:        'apiErrors.familyCannotLeaveDefault',
  FAMILY_CANNOT_LEAVE_LAST_HEAD:      'apiErrors.familyCannotLeaveLastHead',
  FAMILY_FORBIDDEN:                   'apiErrors.familyForbidden',
  TAG_NOT_VISIBLE:                    'apiErrors.tagNotVisible',
  FAMILY_INVITATION_INVALID:          'apiErrors.familyInvitationInvalid',
  FAMILY_INVITATION_ALREADY_ACCEPTED: 'apiErrors.familyInvitationAlreadyAccepted',
  FAMILY_INVITATION_EXPIRED:          'apiErrors.familyInvitationExpired',
  FAMILY_NAME_ALREADY_EXISTS:         'apiErrors.familyNameAlreadyExists',
}

export const BACKEND_ERROR_CODES: Record<string, string> = new Proxy(BACKEND_KEYS, {
  get(target, key: string) {
    return key in target ? i18next.t(target[key]) : undefined
  },
})
