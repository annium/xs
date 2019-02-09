import { Provider } from 'mobx-react'
import * as React from 'react'
import * as ReactDOM from 'react-dom'

import Routes from './routes'
import createStore from './store'
import './styles/layout.scss'


const store = createStore()

ReactDOM.render(
  <Provider {...store}>
    <Routes />
  </Provider>,
  document.getElementById('root')
)

Object.defineProperty(window, 's', { get: () => store })