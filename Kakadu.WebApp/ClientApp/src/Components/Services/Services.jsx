import React from 'react';
import { connect } from 'react-redux';
import { Container, Table, Navbar, NavItem, NavLink } from 'reactstrap';
import { serviceActions } from '../../_actions';
import { Link } from 'react-router-dom';

class Services extends React.Component {
    componentDidMount() {
        this.props.dispatch(serviceActions.getAll());
    }

    render() {
        const { services } = this.props;
        return (
            <Container>
                {services.error && <span className="text-danger">ERROR: {services.error}</span>}
                <Navbar className="navbar-expand-sm navbar-toggleable-sm pl-0 mb-0" light>
                    <ul className="navbar-nav mr-auto">
                    <NavItem>
                        <NavLink className="btn btn-success text-white" to="/services/add">Add service</NavLink>
                    </NavItem>
                    </ul>
                </Navbar>
                <Table bordered hover responsive>
                    <thead>
                        <tr>
                            <th>Code</th>
                            <th>Name</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {services.loading && <tr><td colSpan="3">Loading services...</td></tr>}    
                        {services.items && services.items.map((service, index) =>
                            <tr key={service.id}>
                                <td>
                                    <Link to="/services/edit">{service.code}</Link>
                                </td>
                                <td>{service.name}</td>
                                <td>start capturing</td>
                            </tr>
                        )}
                    </tbody>
                </Table>
            </Container>
        );
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

const connectedServicesPage = connect(mapStateToProps)(Services);
export { connectedServicesPage as Services };