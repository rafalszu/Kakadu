import React, { Component } from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { connect } from 'react-redux';
import { Link } from 'react-router-dom';
import './NavMenu.css';

class NavMenu extends Component {
  static displayName = NavMenu.name;

  constructor (props) {
    super(props);

    this.toggleNavbar = this.toggleNavbar.bind(this);
    this.state = {
      collapsed: true
    };
  }

  toggleNavbar () {
    this.setState({
      collapsed: !this.state.collapsed
    });
  }

  render () {
    const { user } = this.props;
    return (
      <header>
        <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" light>
          <Container>
          {/*  */}
            <NavbarBrand href="/" to="/">Kakadu</NavbarBrand> 

            <NavbarToggler onClick={this.toggleNavbar} className="mr-2" />
            <Collapse className="collapse navbar-collapse justify-content-between" isOpen={!this.state.collapsed} navbar>
              <ul className="navbar-nav mr-auto">
                <NavItem>
                  <NavLink href="/services" className="text-dark" to="/services">Services</NavLink>
                </NavItem>
              </ul>
              {user &&
                <ul className="navbar-nav">
                  <NavItem>
                    <NavLink href="/login" className="btn btn-primary text-white" to="/login">Log out</NavLink>
                  </NavItem>
                </ul>
              }
            </Collapse>
          </Container>
        </Navbar>
      </header>
    );
  }
}

function mapStateToProps(state) {
  console.log(state);
  const { authentication } = state;
  const { user } = authentication;
  return {
      user
  };
}

const connectedNavBar = connect(mapStateToProps)(NavMenu);
export { connectedNavBar as NavMenu };