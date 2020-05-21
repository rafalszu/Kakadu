import React from 'react';
import { connect } from 'react-redux';
import { Container, Table, Navbar, NavItem, NavLink } from 'reactstrap';
import { serviceActions } from '../../_actions';

class Edit extends React.Component {
    componentDidMount() {
        //this.props.dispatch(serviceActions.getAll());
        const {serviceCode} = this.props.match.params;
        this.props.dispatch(serviceActions.getByCode(serviceCode));
    }

    render() {
        return (
            <Container>
                <h3>services edit component</h3>
            </Container>
        )
    }
}


function mapStateToProps(state) {
    const { services, authentication } = state;
    const { user } = authentication;
    return {
        user,
        services
    };
}

const connectedServicesPage = connect(mapStateToProps)(Edit);
export { connectedServicesPage as Edit };