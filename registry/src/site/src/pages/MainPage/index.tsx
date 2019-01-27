import * as React from 'react'


const log = console.log.bind(console, 'MainPage')
export default class MainPage extends React.Component {
  render() {
    log('render')
    return (
      <div className="main-page">
        Main page
      </div>
    )
  }
}