import { I18nProvider } from '@lingui/react'
import React from 'react'
import ReactDOM from 'react-dom'

import { StoreProvider } from './stores'
import { i18n } from './i18n'
import { Routes } from './routes'


ReactDOM.render(
  (
    <StoreProvider>
      <I18nProvider i18n={i18n} language={i18n.language}>
        <Routes />
      </I18nProvider>
    </StoreProvider>
  ),
  document.getElementById('root'),
)
