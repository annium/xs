import React from 'react'
import ReactDOM from 'react-dom'

import { context } from './context'
import { Routes } from './routes'
// tslint:disable-next-line:no-import-side-effect
import './styles/layout.scss'


ReactDOM.render(<Routes />, document.getElementById('root'))

Object.defineProperty(window, 's', { get: () => context.getState })
