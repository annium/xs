import { createBrowserHistory } from 'history'
import React from 'react'
import { Route, Router, Switch } from 'react-router-dom'

import { App } from './App'
import { HomePage } from './pages/HomePage'

const navHistory = createBrowserHistory()


const renderNotFound = () => <h1>Not found</h1>

// tslint:disable-next-line:no-any
const renderApp = (props: any) => (
  // tslint:disable-next-line:no-unsafe-any
  <App {...props}>
    <Switch>
      <Route path="/" exact={true} component={HomePage} />
      <Route render={renderNotFound} />
    </Switch>
  </App>
)

export const Routes = () => (
  <Router history={navHistory}>
    <Route render={renderApp} />
  </Router>
)
