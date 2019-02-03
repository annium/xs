import * as React from 'react'
import { Route, Router, Switch } from 'react-router-dom'

import App from './App'
import PersonalArea from './PersonalArea'
import LoginPage from './pages/LoginPage'
import MainPage from './pages/MainPage'
import SettingsPage from './pages/SettingsPage'

import createBrowserHistory from 'history/createBrowserHistory'
const navHistory = createBrowserHistory()


const Routes: React.SFC = () => (
  <Router history={navHistory}>
    <Route render={renderApp} />
  </Router>
)

const renderApp = (props: any) => (
  <App {...props}>
    <Switch>
      <Route path="/login" exact component={LoginPage} />
      <Route render={renderPersonalArea} />
    </Switch>
  </App>
)

const renderPersonalArea = (props: any) => (
  <PersonalArea {...props}>
    <Switch>
      <Route path="/" exact component={MainPage} />
      <Route path="/settings" exact component={SettingsPage} />
      <Route render={() => <h1>Not found</h1>} />
    </Switch>
  </PersonalArea>
)

export default Routes