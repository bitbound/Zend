import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { NavMenu } from './NavMenu';

export class Layout extends Component {
  static displayName = Layout.name;

  render() {
    return (
      <div>
        <h2 className="text-info mt-1 ml-2 mb-0">
          Zend
        </h2>
        <div>
          <small className="text-dark text-muted ml-2">
              Soothingly simple file transfer.
          </small>
        </div>

        {/*<NavMenu />*/}
        <Container>
          {this.props.children}
        </Container>
      </div>
    );
  }
}
