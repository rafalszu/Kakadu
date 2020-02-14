import React from 'react';
import { connect } from 'react-redux';
import { Container, Table } from 'reactstrap';

import { serviceActions } from '../../_actions';

class Services extends React.Component {
    componentDidMount() {
        this.props.dispatch(serviceActions.getAll());
    }

    render() {
        const { services } = this.props;
        return (
            <Container>
                {services.error && <span className="text-danger">ERROR: {services.error}</span>}
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
                                <td scope="row">{service.code}</td>
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