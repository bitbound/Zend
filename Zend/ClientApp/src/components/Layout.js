import React, { Component } from 'react';
import { Container } from 'reactstrap';

export class Layout extends Component {
  static displayName = Layout.name;

  render() {
    return (
      <div>
        <h2 className="text-info mt-1 ml-2 mb-0">
          Zend
        </h2>
     
        <div className="text-dark text-muted ml-2 small">
          Soothingly simple file transfer.
        </div>
        <div className="ml-2 small">
          <a href="swagger" target="_blank" rel="noopener noreferrer">API Reference</a>
        </div>

        {/*<NavMenu />*/}
        <Container>
          {this.props.children}
        </Container>
      </div>
    );
  }
}
