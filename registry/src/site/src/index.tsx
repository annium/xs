import * as React from 'react'
import * as ReactDOM from 'react-dom'
import { Provider } from 'mobx-react'

import Routes from './routes'


import './styles/layout.scss'


import createStore from './store'
const store = createStore()

ReactDOM.render(
  <Provider {...store}>
    <Routes />
  </Provider>,
  document.getElementById('root')
)

Object.defineProperty(window, 's', { get: () => store })