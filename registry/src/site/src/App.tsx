import * as React from 'react'
import { Component } from 'react'

import logo from './logo.svg'


import styles from './App.module.scss'


import * as lib from 'site.lib'

export default class App extends Component {
  private api: lib.api.Client

  constructor(props: {}) {
    super(props)
    this.api = lib.api.factory({ url: 'http://localhost:9901' })
  }

  handleClick = async () => {
    console.log('send request')
    const regsitry = await this.api.get('registry');
    console.log('regsitry info:', regsitry)
  }

  render() {
    return (
      <div className={styles.container}>
        <header className={styles.header}>
          <img src={logo} className={styles.logo} alt="logo" />
          <p>Edit <code>src/App.tsx</code> and save to reload.</p>
          <a className={styles.link} href="https://reactjs.org" target="_blank" rel="noopener noreferrer">Learn React</a>
          <button onClick={this.handleClick}>site.lib</button>
        </header>
      </div>
    );
  }
}