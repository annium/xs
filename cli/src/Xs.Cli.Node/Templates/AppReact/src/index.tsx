import { I18nProvider } from '@lingui/react'
import React from 'react'
import ReactDOM from 'react-dom'

import { context } from './context'
import { i18n } from './i18n'
import { Routes } from './routes'


ReactDOM.render(
  (
    <I18nProvider i18n={i18n} language={i18n.language}>
      <Routes />
    </I18nProvider>
  ),
  document.getElementById('root'),
)

Object.defineProperty(window, 's', { get: context.getState })
