import { Provider } from 'mobx-react'
import * as React from 'react'
import * as ReactDOM from 'react-dom'

import { Routes } from './routes'
import { createStore } from './store'
// tslint:disable-next-line:no-import-side-effect
import './styles/layout.scss'


export const store = createStore()

ReactDOM.render(
  <Provider {...store}>
    <Routes />
  </Provider>,
  document.getElementById('root'),
)

Object.defineProperty(window, 's', { get: () => store })
