import * as React from 'react'
import { Component } from 'react'

import logo from './logo.svg'


import styles from './App.module.scss'


import { demo } from 'libdemo';

export default class App extends Component {
  handleClick = () => {
    demo('lib');
  }

  render() {
    return (
      <div className={styles.container}>
        <header className={styles.header}>
          <img src={logo} className={styles.logo} alt="logo" />
          <p>Edit <code>src/App.tsx</code> and save to reload.</p>
          <a className={styles.link} href="https://reactjs.org" target="_blank" rel="noopener noreferrer">Learn React</a>
          <button onClick={this.handleClick}>libdemo</button>
        </header>
      </div>
    );
  }
}