import { Provider } from 'mobx-react'
import React from 'react'
import ReactDOM from 'react-dom'

import { Routes } from './routes'
import { store } from './store'
// tslint:disable-next-line:no-import-side-effect
import './styles/layout.scss'


ReactDOM.render(
  <Provider {...store}>
    <Routes />
  </Provider>,
  document.getElementById('root'),
)

Object.defineProperty(window, 's', { get: () => store })
