import { setupI18n } from '@lingui/core'

const catalogs = {
  en: require('./locales/en/messages'),
}

const language = 'en'

const missing = (lang: string, id: string) => {
  const message = `No translation for '${id}' in '${lang}'`
  alert(message)

  return message
}

export const i18n = setupI18n({ catalogs, language, missing, })
